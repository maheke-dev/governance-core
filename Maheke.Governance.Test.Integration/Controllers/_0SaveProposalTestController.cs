using Microsoft.AspNetCore.Mvc.Testing;
using Maheke.Gov.Test.Integration.Fixtures;
using Maheke.Gov.Test.Integration.Helpers;
using Maheke.Gov.WebApi;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Maheke.Gov.Test.Integration.Controllers
{
    [Collection("Stellar collection")]
    [TestCaseOrderer(
        "Maheke.Gov.Test.Integration.Helpers.AlphabeticalOrderer",
        "Maheke.Gov.Test.Integration"
    )]
    public class _0SaveProposalTestController : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public _0SaveProposalTestController(
            WebApplicationFactory<Program> factory,
            StellarFixture fixture,
            ITestOutputHelper testOutputHelper
        )
        {
            _factory = factory;
            Config = fixture.Config;
            _testOutputHelper = testOutputHelper;
        }

        private TestConfiguration Config { get; }

        [Fact]
        public async Task Test_00_Save_Proposal()
        {
            var proposalCreator = await StellarHelper.CreateProposalCreatorAccount();
            await StellarHelper.CreateFeesPaymentClaimableBalance(
                proposalCreator,
                Config.MahekeMicropaymentSenderKeyPair
            );
            var requestContent =
                $@"{{""name"": ""Proposal1NameTest"", ""description"": ""A testing proposal"", ""creator"": ""{proposalCreator.AccountId}""}}";

            var httpClient = _factory.CreateClient();

            await MahekeHelper.SaveProposal(httpClient, requestContent);
            var proposalList = (await MahekeHelper.GetList(httpClient, Config)).Items;
            var proposal = await MahekeHelper.GetProposalByAssetCode(
                httpClient,
                proposalList.Last().Id
            );
            var whitelistedAssets = proposal.WhitelistedAssets.ToArray();

            Assert.Equal("Proposal1NameTest", proposal.Name);
            Assert.Equal("A testing proposal", proposal.Description);
            Assert.Equal(proposalCreator.AccountId, proposal.Creator);
            Assert.Equal(proposal.Deadline, proposal.Created.Date.AddDays(31));
            Assert.Equal("USDC", whitelistedAssets[0].Asset.Code);
            Assert.Equal(1.0m, whitelistedAssets[0].Multiplier);
            Assert.Equal(
                "9999.9991000",
                await StellarHelper.GetAccountXlmBalance(proposal.Creator)
            );
            Assert.Equal(
                "10000.0000000",
                await StellarHelper.GetAccountXlmBalance(Config.MahekeMicropaymentSenderPublic)
            );
        }

        [Fact]
        public async Task Test_01_Save_Proposal_Throws_Error_If_Name_Exceeds_28_Characters()
        {
            var requestContent =
                $@"{{""name"": ""A proposal name that exceeds 28 characters"", ""description"": ""A testing proposal"", ""creator"": ""Creator""}}";

            var httpClient = _factory.CreateClient();
            var response = await MahekeHelper.SaveProposal(httpClient, requestContent);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains(
                "The proposal name cannot exceed 28 characters",
                response.Content.ReadAsStringAsync().Result
            );
        }

        [Fact]
        public async Task Test_02_Save_Proposal_3500_Characters()
        {
            var proposalCreator = await StellarHelper.CreateProposalCreatorAccount();

            await StellarHelper.CreateFeesPaymentClaimableBalance(
                proposalCreator,
                Config.MahekeMicropaymentSenderKeyPair
            );
            const string proposalDescription =
                "1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222233333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333334444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555566666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666";

            var requestContent =
                $@"{{""name"": ""Proposal"", ""description"": ""{proposalDescription}"", ""creator"": ""{proposalCreator.AccountId}""}}";

            var httpClient = _factory.CreateClient();
            await MahekeHelper.SaveProposal(httpClient, requestContent);
            var proposalList = (await MahekeHelper.GetList(httpClient, Config)).Items;
            var proposal = await MahekeHelper.GetProposalByAssetCode(
                httpClient,
                proposalList.Last().Id
            );
            var whitelistedAssets = proposal.WhitelistedAssets.ToArray();

            Assert.Equal("Proposal", proposal.Name);
            Assert.Equal(proposalDescription, proposal.Description);
            Assert.Equal(proposalCreator.AccountId, proposal.Creator);
            Assert.Equal(proposal.Deadline, proposal.Created.Date.AddDays(31));
            Assert.Equal("USDC", whitelistedAssets[0].Asset.Code);
            Assert.Equal(1.0m, whitelistedAssets[0].Multiplier);
            Assert.Equal(
                "9999.9936200",
                await StellarHelper.GetAccountXlmBalance(proposal.Creator)
            );
            Assert.Equal(
                "10000.0000000",
                await StellarHelper.GetAccountXlmBalance(Config.MahekeMicropaymentSenderPublic)
            );
        }

        [Fact]
        public async Task Test_03_Concurrently_Save_Two_Proposals()
        {
            var proposalCreator1 = await StellarHelper.CreateProposalCreatorAccount();
            var proposalCreator2 = await StellarHelper.CreateProposalCreatorAccount();

            var requestContent =
                $@"{{""name"": ""ConcurrentProposal1"", ""description"": ""A testing proposal"", ""creator"": ""{proposalCreator1.AccountId}""}}";

            var requestContent2 =
                $@"{{""name"": ""ConcurrentProposal2"", ""description"": ""A testing proposal"", ""creator"": ""{proposalCreator2.AccountId}""}}";

            await StellarHelper.CreateFeesPaymentClaimableBalance(
                proposalCreator1,
                Config.MahekeMicropaymentSenderKeyPair
            );
            await StellarHelper.CreateFeesPaymentClaimableBalance(
                proposalCreator2,
                Config.MahekeMicropaymentSenderKeyPair
            );

            var httpClient = _factory.CreateClient();

            Task.WaitAll(
                MahekeHelper.SaveProposal(httpClient, requestContent),
                MahekeHelper.SaveProposal(httpClient, requestContent2)
            );

            var proposalIdentifierList = (await MahekeHelper.GetList(httpClient, Config)).Items;

            Assert.Contains(proposalIdentifierList, p => p.Name == "ConcurrentProposal1");
            Assert.Contains(proposalIdentifierList, p => p.Name == "ConcurrentProposal2");
        }

        [Fact]
        public async Task Test_04_Save_Proposal_Throws_Error_If_There_Is_No_Claimable_Balance_To_Cover_Costs()
        {
            var proposalCreator = await StellarHelper.CreateProposalCreatorAccount();

            var requestContent =
                $@"{{""name"": ""Proposal"", ""description"": ""A testing proposal"", ""creator"": ""{proposalCreator.AccountId}""}}";

            var httpClient = _factory.CreateClient();
            var response = await MahekeHelper.SaveProposal(httpClient, requestContent);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains(
                "No claimable balance found",
                response.Content.ReadAsStringAsync().Result
            );
        }

        [Fact]
        public async Task Test_05_Save_Proposal_Throws_Error_If_Claimable_Balance_Reclaim_Fails()
        {
            var proposalCreator = await StellarHelper.CreateProposalCreatorAccount();

            var requestContent =
                $@"{{""name"": ""Proposal"", ""description"": ""A testing proposal"", ""creator"": ""{proposalCreator.AccountId}""}}";

            await StellarHelper.CreateInvalidFeesPaymentClaimableBalance(
                proposalCreator,
                Config.MahekeMicropaymentSenderKeyPair
            );

            var httpClient = _factory.CreateClient();
            var response = await MahekeHelper.SaveProposal(httpClient, requestContent);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains(
                "Error claiming the claimable balance: tx_failed",
                response.Content.ReadAsStringAsync().Result
            );
            await StellarHelper.ClaimClaimableBalance(proposalCreator.SecretSeed);
        }
    }
}
