using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace Maheke.Gov.Test.Integration.Helpers
{
    public static class NetworkSetup
    {
        public static async Task Setup(Server server, TestConfiguration configuration)
        {
            StellarHelper.Server = server;
            StellarHelper.MasterAccount = KeyPair.FromSecretSeed(configuration.MasterAccountPrivate);

            await CreateMahekeAccounts(configuration);
            await CreateProposalCreatorAccounts(configuration);
            await CreateStableCoinIssuerAccounts(configuration);
            await FundVoter(configuration);

            PrintConfigurationValues(
                new[]
                {
                    configuration.MahekeMicropaymentSenderKeyPair,
                    configuration.MahekeMicropaymentReceiverKeyPair,
                    configuration.MahekeEscrowKeyPair,
                    configuration.MahekeResultsKeyPair,
                    configuration.UsdcAssetIssuerKeyPair,
                    configuration.YusdcAssetIssuerKeyPair,

                    configuration.ProposalCreator1KeyPair,
                    configuration.ProposalCreator2KeyPair,
                    configuration.VoterKeyPair
                },
                new[]
                {
                    TestConfiguration.MahekeMicropaymentSenderConfigKey,
                    TestConfiguration.MahekeMicropaymentReceiverConfigKey,
                    TestConfiguration.MahekeEscrowConfigKey,
                    TestConfiguration.MahekeResultsConfigKey,
                    TestConfiguration.UsdcAssetIssuerConfigKey,
                    TestConfiguration.YusdcAssetIssuerConfigKey,

                    TestConfiguration.ProposalCreator1ConfigKey,
                    TestConfiguration.ProposalCreator2ConfigKey,
                    TestConfiguration.VoterConfigKey
                },
                configuration.BaseConfigFile
            );
        }

        private static async Task CreateMahekeAccounts(TestConfiguration configuration)
        {
            // MAHEKE SENDER
            configuration.MahekeMicropaymentSenderKeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.MahekeMicropaymentSenderConfigKey,
                "Main Maheke Micropayment Sender Account", configuration.MahekeMicropaymentSenderPrivate
            );
            configuration.MahekeMicropaymentSenderPublic = configuration.MahekeMicropaymentSenderKeyPair.AccountId;
            configuration.MahekeMicropaymentSenderPrivate =
                configuration.MahekeMicropaymentSenderKeyPair.SecretSeed;

            // MAHEKE RECEIVER
            configuration.MahekeMicropaymentReceiverKeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.MahekeMicropaymentReceiverConfigKey,
                "Main Maheke Micropayment Receiver Account", configuration.MahekeMicropaymentReceiverPrivate
            );
            configuration.MahekeMicropaymentReceiverPublic =
                configuration.MahekeMicropaymentReceiverKeyPair.AccountId;
            configuration.MahekeMicropaymentReceiverPrivate =
                configuration.MahekeMicropaymentReceiverKeyPair.SecretSeed;

            // MAHEKE ESCROW
            configuration.MahekeEscrowKeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.MahekeEscrowConfigKey, "Maheke escrow account",
                configuration.MahekeEscrowPrivate);
            configuration.MahekeEscrowPublic = configuration.MahekeEscrowKeyPair.AccountId;
            configuration.MahekeEscrowPrivate = configuration.MahekeEscrowKeyPair.SecretSeed;

            // MAHEKE RESULTS
            configuration.MahekeResultsKeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.MahekeResultsConfigKey, "Maheke escrow account",
                configuration.MahekeResultsPrivate);
            configuration.MahekeResultsPublic = configuration.MahekeResultsKeyPair.AccountId;
            configuration.MahekeResultsPrivate = configuration.MahekeResultsKeyPair.SecretSeed;
        }

        private static async Task CreateProposalCreatorAccounts(TestConfiguration configuration)
        {
            //PROPOSAL CREATOR 1
            configuration.ProposalCreator1KeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.ProposalCreator1ConfigKey, "Proposal1Creator account",
                configuration.ProposalCreator1Private);
            configuration.ProposalCreator1Public = configuration.ProposalCreator1KeyPair.AccountId;
            configuration.ProposalCreator1Private = configuration.ProposalCreator1KeyPair.SecretSeed;

            //PROPOSAL CREATOR 2
            configuration.ProposalCreator2KeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.ProposalCreator2ConfigKey, "Proposal2Creator account",
                configuration.ProposalCreator2Private);
            configuration.ProposalCreator2Public = configuration.ProposalCreator2KeyPair.AccountId;
            configuration.ProposalCreator2Private = configuration.ProposalCreator2KeyPair.SecretSeed;

            //VOTER
            configuration.VoterKeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.VoterConfigKey, "Voter account",
                configuration.VoterPrivate);
            configuration.VoterPublic = configuration.VoterKeyPair.AccountId;
            configuration.VoterPrivate = configuration.VoterKeyPair.SecretSeed;
        }

        private static async Task CreateStableCoinIssuerAccounts(TestConfiguration configuration)
        {
            configuration.YusdcAssetIssuerKeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.YusdcAssetIssuerConfigKey, "YusdcAssetIssuer account",
                configuration.YusdcAssetIssuerPrivate);
            configuration.YusdcAssetIssuerPublic = configuration.YusdcAssetIssuerKeyPair.AccountId;
            configuration.YusdcAssetIssuerPrivate = configuration.YusdcAssetIssuerKeyPair.SecretSeed;
            configuration.YusdcAsset = new AssetTypeCreditAlphaNum12("yUSDC", configuration.YusdcAssetIssuerPublic);

            configuration.UsdcAssetIssuerKeyPair = await StellarHelper.GetOrCreateAccountKeyPair(
                TestConfiguration.UsdcAssetIssuerConfigKey, "UsdcAssetIssuer account",
                configuration.UsdcAssetIssuerPrivate);
            configuration.UsdcAssetIssuerPublic = configuration.UsdcAssetIssuerKeyPair.AccountId;
            configuration.UsdcAssetIssuerPrivate = configuration.UsdcAssetIssuerKeyPair.SecretSeed;
            configuration.UsdcAsset = new AssetTypeCreditAlphaNum4("USDC", configuration.UsdcAssetIssuerPublic);
        }

        private static async Task FundVoter(TestConfiguration configuration)
        {
            await StellarHelper.Pay(
                configuration.UsdcAssetIssuerKeyPair,
                configuration.VoterKeyPair,
                configuration.UsdcAsset.Code,
                10000
            );
        }

        private static void PrintConfigurationValues(
            IReadOnlyCollection<KeyPair> accountKeyPairs,
            string[] descriptions,
            string baseConfigFile
        )
        {
            var template = File.ReadAllText(baseConfigFile);
            var configObject = JObject.Parse(template);

            for (var i = 0; i < accountKeyPairs.Count; i++)
            {
                var configKeyBase = descriptions.ElementAt(i);
                var publicKeyConfigKey = $"{configKeyBase}_PUBLIC_KEY";
                var privateKeyConfigKey = $"{configKeyBase}_PRIVATE_KEY";
                var keyPair = accountKeyPairs.ElementAt(i);
                var publicKey = keyPair.AccountId;
                var privateKey = keyPair.SecretSeed;
                Environment.SetEnvironmentVariable(publicKeyConfigKey, publicKey);
                Environment.SetEnvironmentVariable(privateKeyConfigKey, privateKey);
                Console.WriteLine($"\"{publicKeyConfigKey}\" : \"{publicKey}\",");
                Console.WriteLine($"\"{privateKeyConfigKey}\" : \"{privateKey}\",");
                configObject.Add(publicKeyConfigKey, publicKey);
                configObject.Add(privateKeyConfigKey, privateKey);
            }

            File.WriteAllText(@"./appsettings.test-result.json", configObject.ToString());
        }
    }
}
