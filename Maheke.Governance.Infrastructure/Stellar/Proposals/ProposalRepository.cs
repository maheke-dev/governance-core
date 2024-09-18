using Newtonsoft.Json;
using Maheke.Gov.Application.Proposals;
using Maheke.Gov.Application.Proposals.Responses;
using Maheke.Gov.Domain;
using Maheke.Gov.Infrastructure.Stellar.Exceptions;
using Maheke.Gov.Infrastructure.Stellar.Helpers;
using System.Globalization;
using Asset = StellarDotnetSdk.Assets;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Memos;

namespace Maheke.Gov.Infrastructure.Stellar.Proposals
{
    public class ProposalRepository : IProposalRepository
    {
        private readonly Server _server;
        private readonly SystemAccountConfiguration _systemAccountConfiguration;

        public ProposalRepository(
            SystemAccountConfiguration systemAccountConfiguration,
            Server server
        )
        {
            _systemAccountConfiguration = systemAccountConfiguration;
            _server = server;
        }

        public async Task SaveProposal(Proposal proposal)
        {
            const int maximumProposalLength = 637;
            if (proposal.Name.Length > EncodingHelper.MemoTextMaximumCharacters)
                throw new ArgumentOutOfRangeException(
                    "The proposal name cannot exceed 28 characters"
                );

            var serializedProposal = JsonConvert.SerializeObject(
                proposal,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK"
                }
            );

            var proposalMicropaymentSenderKeyPair = KeyPair.FromSecretSeed(
                _systemAccountConfiguration.MicropaymentSenderPrivateKey
            );
            var proposalMicropaymentReceiverKeyPair = KeyPair.FromSecretSeed(
                _systemAccountConfiguration.MicropaymentReceiverPrivateKey
            );
            var micropaymentSenderAccountResponse = await _server.Accounts.Account(
                proposalMicropaymentSenderKeyPair.AccountId
            );
            var micropaymentSenderAccount = new Account(
                proposalMicropaymentSenderKeyPair.AccountId,
                micropaymentSenderAccountResponse.SequenceNumber
            );
            var proposalCreatorAccount = await _server.Accounts.Account(proposal.Creator);
            var assetCode = EncodingHelper.EncodeSeqNumberToBase48(
                proposalCreatorAccount.SequenceNumber
            );
            var proposalMicropaymentSenderInitialXlmBalance = await GetAccountXlmBalance(
                proposalMicropaymentSenderKeyPair.AccountId
            );

            SubmitTransactionResponse claimClaimableBalanceResponse;
            try
            {
                claimClaimableBalanceResponse = await ClaimClaimableBalance(
                    micropaymentSenderAccount,
                    proposalMicropaymentSenderKeyPair,
                    proposal.Creator
                );
            }
            catch (InvalidOperationException)
            {
                throw new ClaimableBalanceException("No claimable balance found");
            }

            while (claimClaimableBalanceResponse.Result is TransactionResultBadSeq)
                claimClaimableBalanceResponse = await ClaimClaimableBalance(
                    micropaymentSenderAccount,
                    proposalMicropaymentSenderKeyPair,
                    proposal.Creator
                );

            if (!claimClaimableBalanceResponse.IsSuccess)
                throw new ClaimableBalanceException(
                    $"Error claiming the claimable balance: {claimClaimableBalanceResponse.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode}"
                );

            for (var i = 0; i <= serializedProposal.Length; i += maximumProposalLength)
            {
                var serializedProposalSection = serializedProposal.Substring(
                    i,
                    serializedProposal.Length - i > maximumProposalLength
                        ? maximumProposalLength
                        : serializedProposal.Length - i
                );

                var transactionResponse = await SaveProposal(
                    serializedProposalSection,
                    proposal.Name,
                    assetCode,
                    proposalMicropaymentSenderKeyPair,
                    proposalMicropaymentReceiverKeyPair,
                    micropaymentSenderAccount
                );

                while (transactionResponse.Result is TransactionResultBadSeq)
                    transactionResponse = await SaveProposal(
                        serializedProposalSection,
                        proposal.Name,
                        assetCode,
                        proposalMicropaymentSenderKeyPair,
                        proposalMicropaymentReceiverKeyPair,
                        micropaymentSenderAccount
                    );

                if (!transactionResponse.IsSuccess)
                    throw new ApplicationException(
                        transactionResponse.SubmitTransactionResponseExtras.ExtrasResultCodes.OperationsResultCodes.Aggregate(
                            "",
                            (acc, code) => $"{acc}, {code}"
                        )
                    );
            }

            await PayBackExceedingFunds(
                proposalMicropaymentSenderInitialXlmBalance,
                micropaymentSenderAccount,
                proposalMicropaymentSenderKeyPair,
                proposal.Creator
            );
        }

        public async Task<Proposal> GetProposal(string assetCode)
        {
            IList<object> transactionHashesAll = new List<object>();
            var proposalMicropaymentReceiverPublicKey = KeyPair
                .FromSecretSeed(_systemAccountConfiguration.MicropaymentReceiverPrivateKey)
                .AccountId;
            IList<PaymentOperationResponse> retrievedRecords = new List<PaymentOperationResponse>();

            var response = await _server.Payments
                .ForAccount(proposalMicropaymentReceiverPublicKey)
                .Limit(200)
                .Execute();
            while (response.Embedded.Records.Count != 0)
            {
                var paymentRecords = response.Records
                    .OfType<PaymentOperationResponse>()
                    .Where(payment => payment.TransactionSuccessful)
                    .ToList();

                foreach (var record in paymentRecords)
                    if (
                        record.AssetCode == assetCode
                        && record.To == proposalMicropaymentReceiverPublicKey
                        && record.AssetIssuer == proposalMicropaymentReceiverPublicKey
                    )
                    {
                        retrievedRecords.Add(record);
                        transactionHashesAll.Add(record.TransactionHash);
                    }

                response = await response.NextPage();
            }

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonConverterHelper());
            var decodedProposal = EncodingHelper.Decode(retrievedRecords, transactionHashesAll);
            return JsonConvert.DeserializeObject<Proposal>(decodedProposal, settings);
        }

        public async Task<List<ProposalIdentifier>> GetProposalList()
        {
            IList<ProposalIdentifier> proposalList = new List<ProposalIdentifier>();
            var proposalMicropaymentReceiverPublicKey = KeyPair
                .FromSecretSeed(_systemAccountConfiguration.MicropaymentReceiverPrivateKey)
                .AccountId;
            var proposalMicropaymentSenderPublicKey = KeyPair
                .FromSecretSeed(_systemAccountConfiguration.MicropaymentSenderPrivateKey)
                .AccountId;

            var response = await _server.Transactions
                .ForAccount(proposalMicropaymentReceiverPublicKey)
                .Limit(200)
                .Execute();
            var transactionRecords = response.Records.Where(
                transaction => transaction.SourceAccount == proposalMicropaymentSenderPublicKey
            );

            foreach (var record in transactionRecords)
                if (proposalList.All(identifier => identifier.Name != record.MemoValue))
                {
                    var assetCode = (
                        await _server.Payments.ForTransaction(record.Hash).Execute()
                    ).Records
                        .OfType<PaymentOperationResponse>()
                        .First()
                        .AssetCode;

                    var proposalClosingDay = DateTime.Parse(record.CreatedAt).AddDays(31).Date;

                    proposalList.Add(
                        new ProposalIdentifier
                        {
                            Id = assetCode,
                            Name = record.MemoValue,
                            Deadline = proposalClosingDay
                        }
                    );
                }
            return proposalList.ToList();
        }

        private async Task<SubmitTransactionResponse> SaveProposal(
            string serializedProposalSection,
            string proposalName,
            string assetCode,
            KeyPair proposalMicropaymentSenderKeyPair,
            KeyPair proposalMicropaymentReceiverKeyPair,
            Account micropaymentSenderAccount
        )
        {
            var encodedProposalPayments = EncodingHelper.Encode(serializedProposalSection);

            var feeStats = await _server.FeeStats.Execute();
            var txBuilder = new TransactionBuilder(micropaymentSenderAccount);
            var asset = Asset.AssetTypeCreditAlphaNum12.CreateNonNativeAsset(
                assetCode,
                proposalMicropaymentReceiverKeyPair.AccountId
            );

            var changeTrustLineOp = new ChangeTrustOperation(asset, null, micropaymentSenderAccount.KeyPair);
            var paymentOp = new PaymentOperation(
                proposalMicropaymentSenderKeyPair,
                asset,
                EncodingHelper.MaxTokens.ToString(),
                proposalMicropaymentReceiverKeyPair
            );
            txBuilder
                .SetFee((uint)feeStats.FeeCharged.P90)
                .AddOperation(changeTrustLineOp)
                .AddOperation(paymentOp)
                .AddMemo(new MemoText(proposalName));

            foreach (var payment in encodedProposalPayments.EncodedProposalMicropayments)
            {
                var encodedTextPaymentOp = new PaymentOperation(
                    proposalMicropaymentReceiverKeyPair,
                    asset,
                    payment.ToString(CultureInfo.CreateSpecificCulture("en-us")),
                    micropaymentSenderAccount.KeyPair
                );
                txBuilder.AddOperation(encodedTextPaymentOp);
            }

            txBuilder.AddOperation(
                new PaymentOperation(
                    proposalMicropaymentReceiverKeyPair,
                    asset,
                    encodedProposalPayments.ExcessTokens.ToString(
                        CultureInfo.CreateSpecificCulture("en-us")
                    )
                )
            );

            var tx = txBuilder.Build();
            tx.Sign(proposalMicropaymentSenderKeyPair);
            tx.Sign(proposalMicropaymentReceiverKeyPair);

            return await _server.SubmitTransaction(tx);
        }

        private async Task<SubmitTransactionResponse> ClaimClaimableBalance(
            Account proposalMicropaymentSender,
            KeyPair proposalMicropaymentSenderKeyPair,
            string proposalCreator
        )
        {
            var sponsor = KeyPair.FromAccountId(proposalCreator);
            var claimableBalance = await _server.ClaimableBalances
                .ForClaimant(proposalMicropaymentSenderKeyPair)
                .ForAsset(new AssetTypeNative())
                .ForSponsor(sponsor)
                .Execute();
            var balanceId = claimableBalance.Records.First().Id;

            var claimClaimableBalanceOp = new ClaimClaimableBalanceOperation(
                balanceId
            );
            var transactionBuilder = new TransactionBuilder(proposalMicropaymentSender);
            var feeStats = await _server.FeeStats.Execute();
            transactionBuilder
                .SetFee((uint)feeStats.FeeCharged.P90)
                .AddOperation(claimClaimableBalanceOp);
            var tx = transactionBuilder.Build();
            tx.Sign(proposalMicropaymentSenderKeyPair);
            return await _server.SubmitTransaction(tx);
        }

        private async Task PayBackExceedingFunds(
            decimal initialXlmBalance,
            Account source,
            KeyPair sourceKeyPair,
            string destination
        )
        {
            var proposalMicropaymentSenderFinalXlmBalance = await GetAccountXlmBalance(
                source.AccountId
            );
            var returnFundsFee = 0.00001M;
            var exceedingFunds =
                proposalMicropaymentSenderFinalXlmBalance - initialXlmBalance - returnFundsFee;

            var destinationKeyPair = KeyPair.FromAccountId(destination);
            var txBuilder = new TransactionBuilder(source);
            var paymentOp = new PaymentOperation(
                destinationKeyPair,
                new AssetTypeNative(),
                Convert.ToString(exceedingFunds, CultureInfo.InvariantCulture)
            );
            var feeStats = await _server.FeeStats.Execute();
            txBuilder.SetFee((uint)feeStats.FeeCharged.P90).AddOperation(paymentOp);
            var tx = txBuilder.Build();
            tx.Sign(sourceKeyPair);
            await _server.SubmitTransaction(tx);
        }

        private async Task<decimal> GetAccountXlmBalance(string publicKey)
        {
            var account = await _server.Accounts.Account(publicKey);
            var balance = account.Balances
                .First(balance => balance.AssetType == "native")
                .BalanceString;
            return Convert.ToDecimal(balance, CultureInfo.InvariantCulture);
        }

        public async Task<int> GetVotingResult(string assetCode)
        {
            var MahekeResultsPublicKey = KeyPair
                .FromSecretSeed(_systemAccountConfiguration.ResultsPrivateKey)
                .AccountId;
            var proposalMicropaymentReceiverPublicKey = KeyPair
                .FromSecretSeed(_systemAccountConfiguration.MicropaymentReceiverPrivateKey)
                .AccountId;
            var response = await _server.Payments
                .ForAccount(MahekeResultsPublicKey)
                .Limit(200)
                .Execute();
            while (response.Embedded.Records.Count != 0)
            {
                var paymentRecords = response.Records
                    .OfType<PaymentOperationResponse>()
                    .Where(payment => payment.TransactionSuccessful)
                    .ToList();

                foreach (var record in paymentRecords)
                    if (
                        record.AssetCode == assetCode
                        && record.To == MahekeResultsPublicKey
                        && record.AssetIssuer == proposalMicropaymentReceiverPublicKey
                    )
                        return Convert.ToInt32(
                            decimal.Parse(record.Amount, CultureInfo.InvariantCulture)
                        );

                response = await response.NextPage();
            }

            return -1;
        }
    }
}
