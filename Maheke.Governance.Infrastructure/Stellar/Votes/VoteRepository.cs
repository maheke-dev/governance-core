using System;
using System.Linq;
using System.Threading.Tasks;
using Maheke.Gov.Application.Votes;
using Maheke.Gov.Domain;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace Maheke.Gov.Infrastructure.Stellar.Votes
{
    public class VoteRepository : IVoteRepository
    {
        private readonly Server _server;
        private readonly SystemAccountConfiguration _systemAccountConfiguration;

        public VoteRepository(SystemAccountConfiguration systemAccountConfiguration, Server server)
        {
            _systemAccountConfiguration = systemAccountConfiguration;
            _server = server;
        }

        public async Task<string> GetVoteIntent(ValidatedVote validatedVote, Proposal proposal, string proposalId)
        {
            var voteTransaction = await GetVoteTransaction(validatedVote, proposal, proposalId);
            return voteTransaction.ToUnsignedEnvelopeXdrBase64();
        }

        public async Task SaveVote(ValidatedVote validatedVote, Proposal proposal, string proposalId,
            string voterPrivateKey)
        {
            var voterKeyPair = KeyPair.FromSecretSeed(voterPrivateKey);
            var voteTransaction = await GetVoteTransaction(validatedVote, proposal, proposalId);
            voteTransaction.Sign(voterKeyPair);
            var transactionResponse = await _server.SubmitTransaction(voteTransaction);

            if (!transactionResponse.IsSuccess)
                throw new ApplicationException(
                    transactionResponse
                        .SubmitTransactionResponseExtras
                        .ExtrasResultCodes
                        .OperationsResultCodes
                        .Aggregate("", (acc, code) => $"{acc}, {code}")
                );
        }

        private async Task<Transaction> GetVoteTransaction(ValidatedVote validatedVote, Proposal proposal,
            string proposalId)
        {
            var escrowKeyPair = KeyPair.FromSecretSeed(_systemAccountConfiguration.EscrowPrivateKey);
            var voterKeyPair = KeyPair.FromAccountId(validatedVote.Voter);
            var voterAccountResponse = await _server.Accounts.Account(validatedVote.Voter);
            var voterAccount =
                new Account(validatedVote.Voter, voterAccountResponse.SequenceNumber);

            var escrowClaimant = new Claimant
            (
                escrowKeyPair,
                ClaimPredicate.Not(ClaimPredicate.BeforeAbsoluteTime(proposal.Deadline.AddDays(1)))
            );

            var voterClaimant = new Claimant
            (
                voterKeyPair,
                ClaimPredicate.Not(
                    ClaimPredicate.BeforeAbsoluteTime(proposal.Deadline.AddDays(1)))
            );

            Asset asset = validatedVote.Asset.IsNative
                ? new AssetTypeNative()
                : validatedVote.Asset.Code.Length > 4
                    ? new AssetTypeCreditAlphaNum12(validatedVote.Asset.Code, validatedVote.Asset.Issuer.Address)
                    : new AssetTypeCreditAlphaNum4(validatedVote.Asset.Code, validatedVote.Asset.Issuer.Address);

            var feeStats = await _server.FeeStats.Execute();
            var txBuilder = new TransactionBuilder(voterAccount);
            var claimableBalanceOp =
                new CreateClaimableBalanceOperation(asset, $"{validatedVote.Amount}",
                        new[] {escrowClaimant, voterClaimant}, voterKeyPair);
            txBuilder.SetFee((uint) feeStats.FeeCharged.P90).AddOperation(claimableBalanceOp)
                .AddMemo(new MemoText($"{proposalId} {validatedVote.Option.Name}"));

            return txBuilder.Build();
        }
    }
}
