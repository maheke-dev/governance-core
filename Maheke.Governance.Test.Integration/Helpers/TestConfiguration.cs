using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace Maheke.Gov.Test.Integration.Helpers
{
    public class TestConfiguration
    {
        private const string MasterAccountConfigKey = "MASTER_ACCOUNT";
        public const string MahekeMicropaymentSenderConfigKey = "MAHEKE_PROPOSAL_MICROPAYMENT_SENDER_ACCOUNT";
        public const string MahekeMicropaymentReceiverConfigKey = "MAHEKE_PROPOSAL_MICROPAYMENT_RECEIVER_ACCOUNT";
        public const string MahekeEscrowConfigKey = "MAHEKE_ESCROW_ACCOUNT";
        public const string MahekeResultsConfigKey = "MAHEKE_RESULTS_ACCOUNT";
        public const string YusdcAssetIssuerConfigKey = "YUSDC_ISSUER_ACCOUNT";
        public const string UsdcAssetIssuerConfigKey = "USDC_ISSUER_ACCOUNT";
        public const string ProposalCreator1ConfigKey = "TEST_PROPOSAL_CREATOR_1_ACCOUNT";
        public const string ProposalCreator2ConfigKey = "TEST_PROPOSAL_CREATOR_2_ACCOUNT";
        public const string VoterConfigKey = "TEST_VOTER_ACCOUNT";

        public TestConfiguration()
        {
            var fileToLoad = "appsettings.staging.json";
            var baseConfigFile = "appsettings.staging.json.dist";

            if (File.Exists("appsettings.test.json"))
            {
                baseConfigFile = "appsettings.test.json.dist";
                fileToLoad = "appsettings.test.json";
            }

            if (File.Exists("appsettings.dev.json"))
            {
                baseConfigFile = "appsettings.dev.json.dist";
                fileToLoad = "appsettings.dev.json";
            }

            ConfigFile = fileToLoad;
            BaseConfigFile = baseConfigFile;
            Console.WriteLine($"Loading file: {ConfigFile}");
            Console.WriteLine($"Base config file: {BaseConfigFile}");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(fileToLoad, false, true)
                .Build();

            var environment = configuration.GetValue<string>("ENVIRONMENT");
            Environment.SetEnvironmentVariable("ENVIRONMENT", environment);

            var passphrase = configuration.GetValue<string>("HORIZON_NETWORK_PASSPHRASE");
            Network.Use(new Network(passphrase));
            Environment.SetEnvironmentVariable("HORIZON_NETWORK_PASSPHRASE", passphrase);

            var baseFeeInXlm = configuration.GetValue<string>("BASE_FEE_IN_XLM");
            Environment.SetEnvironmentVariable("BASE_FEE_IN_XLM", baseFeeInXlm);

            TestHorizonUrl = configuration.GetValue<string>("HORIZON_URL");
            Environment.SetEnvironmentVariable("HORIZON_URL", TestHorizonUrl);

            MasterAccountPublic = configuration.GetValue<string>(GetPublicConfigKey(MasterAccountConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(MasterAccountConfigKey), MasterAccountPublic);
            MasterAccountPrivate = configuration.GetValue<string>(GetPrivateConfigKey(MasterAccountConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(MasterAccountConfigKey), MasterAccountPrivate);
            KeyPair.FromSecretSeed(MasterAccountPrivate);

            MahekeMicropaymentSenderPublic =
                configuration.GetValue<string>(GetPublicConfigKey(MahekeMicropaymentSenderConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(MahekeMicropaymentSenderConfigKey),
                MahekeMicropaymentSenderPublic);
            MahekeMicropaymentSenderPrivate =
                configuration.GetValue<string>(GetPrivateConfigKey(MahekeMicropaymentSenderConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(MahekeMicropaymentSenderConfigKey),
                MahekeMicropaymentSenderPrivate);
            if (MahekeMicropaymentSenderPrivate != null)
                MahekeMicropaymentSenderKeyPair = KeyPair.FromSecretSeed(MahekeMicropaymentSenderPrivate);

            MahekeMicropaymentReceiverPublic =
                configuration.GetValue<string>(GetPublicConfigKey(MahekeMicropaymentReceiverConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(MahekeMicropaymentReceiverConfigKey),
                MahekeMicropaymentReceiverPublic);
            MahekeMicropaymentReceiverPrivate =
                configuration.GetValue<string>(GetPrivateConfigKey(MahekeMicropaymentReceiverConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(MahekeMicropaymentReceiverConfigKey),
                MahekeMicropaymentReceiverPrivate);
            if (MahekeMicropaymentReceiverPrivate != null)
                MahekeMicropaymentReceiverKeyPair = KeyPair.FromSecretSeed(MahekeMicropaymentReceiverPrivate);

            YusdcAssetIssuerPublic = configuration.GetValue<string>(GetPublicConfigKey(YusdcAssetIssuerConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(YusdcAssetIssuerConfigKey), YusdcAssetIssuerPublic);
            YusdcAssetIssuerPrivate = configuration.GetValue<string>(GetPrivateConfigKey(YusdcAssetIssuerConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(YusdcAssetIssuerConfigKey), YusdcAssetIssuerPrivate);
            if (YusdcAssetIssuerPrivate != null)
                YusdcAssetIssuerKeyPair = KeyPair.FromSecretSeed(YusdcAssetIssuerPrivate);

            UsdcAssetIssuerPublic = configuration.GetValue<string>(GetPublicConfigKey(UsdcAssetIssuerConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(UsdcAssetIssuerConfigKey), UsdcAssetIssuerPublic);
            UsdcAssetIssuerPrivate = configuration.GetValue<string>(GetPrivateConfigKey(UsdcAssetIssuerConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(UsdcAssetIssuerConfigKey), UsdcAssetIssuerPrivate);
            if (UsdcAssetIssuerPrivate != null)
                UsdcAssetIssuerKeyPair = KeyPair.FromSecretSeed(UsdcAssetIssuerPrivate);

            ProposalCreator1Public = configuration.GetValue<string>(GetPublicConfigKey(ProposalCreator1ConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(ProposalCreator1ConfigKey), ProposalCreator1Public);
            ProposalCreator1Private = configuration.GetValue<string>(GetPrivateConfigKey(ProposalCreator1ConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(ProposalCreator1ConfigKey), ProposalCreator1Private);
            if (ProposalCreator1Private != null)
                ProposalCreator1KeyPair = KeyPair.FromSecretSeed(ProposalCreator1Private);

            ProposalCreator2Public = configuration.GetValue<string>(GetPublicConfigKey(ProposalCreator2ConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(ProposalCreator2ConfigKey), ProposalCreator2Public);
            ProposalCreator2Private = configuration.GetValue<string>(GetPrivateConfigKey(ProposalCreator2ConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(ProposalCreator2ConfigKey), ProposalCreator2Private);
            if (ProposalCreator2Private != null)
                ProposalCreator2KeyPair = KeyPair.FromSecretSeed(ProposalCreator2Private);

            MahekeEscrowPublic = configuration.GetValue<string>(GetPublicConfigKey(MahekeEscrowConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(MahekeEscrowConfigKey), MahekeEscrowPublic);
            MahekeEscrowPrivate = configuration.GetValue<string>(GetPrivateConfigKey(MahekeEscrowConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(MahekeEscrowConfigKey), MahekeEscrowPrivate);
            if (MahekeEscrowPrivate != null)
                MahekeEscrowKeyPair = KeyPair.FromSecretSeed(MahekeEscrowPrivate);

            MahekeResultsPublic = configuration.GetValue<string>(GetPublicConfigKey(MahekeResultsConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(MahekeResultsConfigKey), MahekeResultsPublic);
            MahekeResultsPrivate = configuration.GetValue<string>(GetPrivateConfigKey(MahekeResultsConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(MahekeResultsConfigKey), MahekeResultsPrivate);
            if (MahekeResultsPrivate != null)
                MahekeResultsKeyPair = KeyPair.FromSecretSeed(MahekeResultsPrivate);

            VoterPublic = configuration.GetValue<string>(GetPublicConfigKey(VoterConfigKey));
            Environment.SetEnvironmentVariable(GetPublicConfigKey(VoterConfigKey), VoterPublic);
            VoterPrivate = configuration.GetValue<string>(GetPrivateConfigKey(VoterConfigKey));
            Environment.SetEnvironmentVariable(GetPrivateConfigKey(VoterConfigKey), VoterPrivate);
            if (VoterPrivate != null)
                VoterKeyPair = KeyPair.FromSecretSeed(VoterPrivate);
        }

        public string ConfigFile { get; }
        public string BaseConfigFile { get; }
        public AssetTypeCreditAlphaNum YusdcAsset { get; set; } = null!;
        public AssetTypeCreditAlphaNum UsdcAsset { get; set; } = null!;
        public string MasterAccountPrivate { get; }
        private string MasterAccountPublic { get; }
        public string MahekeMicropaymentSenderPublic { get; set; }
        public string? MahekeMicropaymentSenderPrivate { get; set; }
        public KeyPair MahekeMicropaymentSenderKeyPair { get; set; } = null!;
        public string MahekeMicropaymentReceiverPublic { get; set; }
        public string? MahekeMicropaymentReceiverPrivate { get; set; }
        public KeyPair MahekeMicropaymentReceiverKeyPair { get; set; } = null!;
        public string MahekeEscrowPublic { get; set; }
        public string MahekeEscrowPrivate { get; set; }
        public KeyPair MahekeEscrowKeyPair { get; set; } = null!;
        public string MahekeResultsPublic { get; set; }
        public string MahekeResultsPrivate { get; set; }
        public KeyPair MahekeResultsKeyPair { get; set; } = null!;
        public string YusdcAssetIssuerPublic { get; set; }
        public string YusdcAssetIssuerPrivate { get; set; }
        public KeyPair YusdcAssetIssuerKeyPair { get; set; } = null!;
        public string UsdcAssetIssuerPublic { get; set; }
        public string UsdcAssetIssuerPrivate { get; set; }
        public KeyPair UsdcAssetIssuerKeyPair { get; set; } = null!;
        public string ProposalCreator1Public { get; set; }
        public string ProposalCreator1Private { get; set; }
        public KeyPair ProposalCreator1KeyPair { get; set; } = null!;
        public string ProposalCreator2Public { get; set; }
        public string ProposalCreator2Private { get; set; }
        public KeyPair ProposalCreator2KeyPair { get; set; } = null!;
        public string VoterPublic { get; set; }
        public string VoterPrivate { get; set; }
        public KeyPair VoterKeyPair { get; set; } = null!;
        public string TestHorizonUrl { get; }

        private static string GetPrivateConfigKey(string baseString)
        {
            return $"{baseString}_PRIVATE_KEY";
        }

        private static string GetPublicConfigKey(string baseString)
        {
            return $"{baseString}_PUBLIC_KEY";
        }
    }
}
