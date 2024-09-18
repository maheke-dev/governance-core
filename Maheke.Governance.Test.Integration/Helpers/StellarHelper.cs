using StellarDotnetSdk;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets;
using Claimant = StellarDotnetSdk.Claimants.Claimant;
using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Transactions;

namespace Maheke.Gov.Test.Integration.Helpers
{
    public static class StellarHelper
    {
        public static KeyPair MasterAccount { get; set; } = null!;
        public static Server Server { get; set; } = null!;

        public static async Task AddXlmFunds(KeyPair destinationKeyPair)
        {
            var fundAccountOp = new PaymentOperation(destinationKeyPair, new AssetTypeNative(), "10000", MasterAccount);

            var masterAccount = await Server.Accounts.Account(MasterAccount.AccountId);
            var transaction = new TransactionBuilder(masterAccount).AddOperation(fundAccountOp).Build();
            transaction.Sign(MasterAccount);
            await Server.SubmitTransaction(transaction);
        }

        private static async Task FundAccountWithXlm(string address, string env)
        {
            if (env == "STAGING")
            {
                await Server.TestNetFriendBot.FundAccount(address).Execute();
                await Server.Accounts.Account(address);
            }
            else
            {
                var destinationKeyPair = KeyPair.FromAccountId(address);
                var createAccountOp = new CreateAccountOperation(destinationKeyPair, "10000", MasterAccount);

                var masterAccount = await Server.Accounts.Account(MasterAccount.AccountId);
                var transaction = new TransactionBuilder(masterAccount).AddOperation(createAccountOp).Build();
                transaction.Sign(MasterAccount);
                await Server.SubmitTransaction(transaction);
            }

            Console.WriteLine($"Account {address} funded successfully.");
        }

        public static async Task<KeyPair> GetOrCreateAccountKeyPair(
            string key,
            string description,
            string? secret = null
        )
        {
            Console.WriteLine($"Looking for {key} for {description}");

            if (secret != null)
            {
                Console.WriteLine($"Found {description}");
                return KeyPair.FromSecretSeed(secret);
            }

            Console.WriteLine($"Didn't find {description}, an account will be created and funded");
            var pair = KeyPair.Random();
            var env = Environment.GetEnvironmentVariable("ENVIRONMENT")!;
            await FundAccountWithXlm(pair.AccountId, env);

            Console.WriteLine($"{description} secret key is {pair.SecretSeed}");
            Console.WriteLine($"{description} public key is {pair.AccountId}");
            Console.WriteLine($"{pair.AccountId} funded successfully with XLM");

            return pair;
        }

        public static async Task CreateFeesPaymentClaimableBalance(KeyPair proposalCreator, KeyPair destination)
        {
            var proposalCreatorAccountResponse = await Server.Accounts.Account(proposalCreator.AccountId);
            var proposalCreatorAccount =
                new Account(proposalCreator.AccountId, proposalCreatorAccountResponse.SequenceNumber);

            var claimant = new Claimant
            (
                destination.AccountId,
                ClaimPredicate.Unconditional()
            );

            var txBuilder = new TransactionBuilder(proposalCreatorAccount);
            var claimableBalanceOp =
                new CreateClaimableBalanceOperation(new AssetTypeNative(), "5", new[] {claimant}, proposalCreator);
            txBuilder.AddOperation(claimableBalanceOp);

            var tx = txBuilder.Build();
            tx.Sign(proposalCreator);
            await Server.SubmitTransaction(tx);
        }

        public static async Task CreateInvalidFeesPaymentClaimableBalance(KeyPair proposalCreator, KeyPair destination)
        {
            var proposalCreatorAccountResponse = await Server.Accounts.Account(proposalCreator.AccountId);
            var proposalCreatorAccount =
                new Account(proposalCreator.AccountId, proposalCreatorAccountResponse.SequenceNumber);

            var claimants = new[]
            {
                new Claimant
                (
                destination.AccountId,
                   ClaimPredicate.Not(
                        ClaimPredicate.BeforeRelativeTime(new Duration(new Uint64(50000))))
                ),
                new Claimant
                (
                    proposalCreator.AccountId, 
                    ClaimPredicate.Unconditional()
                )
            };

            var txBuilder = new TransactionBuilder(proposalCreatorAccount);
            var claimableBalanceOp =
                new CreateClaimableBalanceOperation(new AssetTypeNative(), "5", claimants);
            txBuilder.AddOperation(claimableBalanceOp);

            var tx = txBuilder.Build();
            tx.Sign(proposalCreator);
            await Server.SubmitTransaction(tx);
        }

        public static async Task<string> GetAccountXlmBalance(string publicKey)
        {
            var account = await Server.Accounts.Account(publicKey);
            return account.Balances.First(balance => balance.AssetType == "native").BalanceString;
        }

        public static async Task<ClaimableBalanceResponse> GetClaimableBalances(string claimantPublicKey,
            string sponsorPublicKey)
        {
            var sponsor = KeyPair.FromAccountId(sponsorPublicKey);
            var claimant = KeyPair.FromAccountId(claimantPublicKey);
            var response = await Server.ClaimableBalances.ForClaimant(claimant).ForSponsor(sponsor).Execute();
            return response.Records.First();
        }

        public static async Task ClaimClaimableBalance(string claimantSecretKey)
        {
            var sponsor = KeyPair.FromSecretSeed(claimantSecretKey);
            var sponsorAccount = await Server.Accounts.Account(sponsor.AccountId);
            var claimableBalance = await Server.ClaimableBalances.ForClaimant(sponsor)
                .ForAsset(new AssetTypeNative()).ForSponsor(sponsor).Execute();
            var balanceId = claimableBalance.Records.First().Id;

            var claimClaimableBalanceOp = new ClaimClaimableBalanceOperation(balanceId);
            var transactionBuilder = new TransactionBuilder(sponsorAccount);
            transactionBuilder.AddOperation(claimClaimableBalanceOp);
            var tx = transactionBuilder.Build();
            tx.Sign(sponsor);
            await Server.SubmitTransaction(tx);
        }

        public static async Task Pay(KeyPair source, KeyPair destinationPublicKey, string assetCode, int amount)
        {
            var sourceAccount = await Server.Accounts.Account(source.AccountId);
            var asset = Asset.AssetTypeCreditAlphaNum12.CreateNonNativeAsset(assetCode, source.AccountId);
            var transactionBuilder = new TransactionBuilder(sourceAccount);
            var trustlineOp = new ChangeTrustOperation(asset, null, destinationPublicKey);
            transactionBuilder.AddOperation(trustlineOp);

            if (amount > 0)
            {
                var paymentOp = new PaymentOperation(destinationPublicKey, asset, amount.ToString());
                transactionBuilder.AddOperation(paymentOp);
            }

            var tx = transactionBuilder.Build();
            tx.Sign(source);
            tx.Sign(destinationPublicKey);
            await Server.SubmitTransaction(tx);
        }

        public static async Task<KeyPair> CreateProposalCreatorAccount()
        {
            return await StellarHelper.GetOrCreateAccountKeyPair("PROPOSAL CREATOR", "Proposal Creator account");
        }
    }
}
