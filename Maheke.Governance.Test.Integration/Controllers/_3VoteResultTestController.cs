using Maheke.Gov.Test.Integration.Fixtures;
using Maheke.Gov.Test.Integration.Helpers;
using Maheke.Gov.WebApi;
using System.Linq;
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
    public class _3VoteResultTestController : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public _3VoteResultTestController(
            CustomWebApplicationFactory<Program> factory,
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
        public async Task Test_00_Get_Voting_Result()
        {
            await StellarHelper.CreateFeesPaymentClaimableBalance(
                Config.ProposalCreator1KeyPair,
                Config.MahekeMicropaymentSenderKeyPair
            );
            var requestContent =
                $@"{{""name"": ""Test_00_Voting_Result"", ""description"": ""A testing proposal"", ""creator"": ""{Config.ProposalCreator1Public}""}}";

            var httpClient = _factory.CreateClient();

            await MahekeHelper.SaveProposal(httpClient, requestContent);
            var proposalList = (await MahekeHelper.GetList(httpClient, Config)).Items;
            var proposalId = proposalList.Last().Id;

            await StellarHelper.Pay(
                Config.MahekeMicropaymentReceiverKeyPair,
                Config.MahekeResultsKeyPair,
                proposalId,
                1
            );
            var proposal = await MahekeHelper.GetProposalByAssetCode(httpClient, proposalId);
            Assert.Equal("FOR", proposal.VotingResult);
        }
    }
}
