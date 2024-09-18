using Microsoft.AspNetCore.Mvc.Testing;
using Maheke.Gov.Test.Integration.Fixtures;
using Maheke.Gov.Test.Integration.Helpers;
using Maheke.Gov.WebApi;
using System;
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
    public class _1GetProposalTestController : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public _1GetProposalTestController(
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
        public async Task Test_00_Get_Proposal_Name_List()
        {
            var proposalCreator1 = await StellarHelper.CreateProposalCreatorAccount();
            var proposalCreator2 = await StellarHelper.CreateProposalCreatorAccount();

            await StellarHelper.CreateFeesPaymentClaimableBalance(
                proposalCreator1,
                Config.MahekeMicropaymentSenderKeyPair
            );
            await StellarHelper.CreateFeesPaymentClaimableBalance(
                proposalCreator2,
                Config.MahekeMicropaymentSenderKeyPair
            );

            var request2Content =
                $@"{{""name"": ""Proposal2NameTest"", ""description"": ""A testing proposal"", ""creator"": ""{proposalCreator1.AccountId}""}}";

            var request3Content =
                $@"{{""name"": ""Proposal3NameTest"", ""description"": ""A testing proposal"", ""creator"": ""{proposalCreator2.AccountId}""}}";

            var httpClient = _factory.CreateClient();
            await MahekeHelper.SaveProposal(httpClient, request2Content);
            await MahekeHelper.SaveProposal(httpClient, request3Content);

            var proposalList = (await MahekeHelper.GetList(httpClient, Config)).Items;
            Assert.Equal(6, proposalList.Count);
            Assert.Equal("Proposal2NameTest", proposalList[4].Name);
            Assert.Equal("Proposal3NameTest", proposalList[5].Name);
            Assert.Equal(DateTime.Today.AddDays(31), proposalList[4].Deadline);
            Assert.Equal(DateTime.Today.AddDays(31), proposalList[5].Deadline);
        }
    }
}
