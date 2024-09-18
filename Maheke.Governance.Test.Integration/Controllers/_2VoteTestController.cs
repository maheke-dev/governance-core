using Microsoft.AspNetCore.Mvc.Testing;
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
    public class _2VoteTestController : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public _2VoteTestController(
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
        public async Task Test_00_Vote()
        {
            await StellarHelper.CreateFeesPaymentClaimableBalance(
                Config.ProposalCreator1KeyPair,
                Config.MahekeMicropaymentSenderKeyPair
            );

            var proposalRequestContent =
                $@"{{""name"": ""Test_00_Vote_Proposal"", ""description"": ""A testing proposal"", ""creator"": ""{Config.ProposalCreator1Public}""}}";

            var httpClient = _factory.CreateClient();
            await MahekeHelper.SaveProposal(httpClient, proposalRequestContent);

            var proposalList = (await MahekeHelper.GetList(httpClient, Config)).Items;
            var proposalId = proposalList.Last().Id;
            var proposal = await MahekeHelper.GetProposalByAssetCode(httpClient, proposalId);

            var transaction = await MahekeHelper.VoteIntent(httpClient, Config, proposalId, "50");
            Assert.Equal($"{proposalId} FOR", transaction.Memo.ToXdr().Text);
            await MahekeHelper.VoteDirect(httpClient, Config, proposalId, "50");

            var claimableBalanceResponse = await StellarHelper.GetClaimableBalances(
                Config.MahekeEscrowPublic,
                Config.VoterPublic
            );
            Assert.Equal(Config.VoterPublic, claimableBalanceResponse.Sponsor);
            Assert.Equal("50.0000000", claimableBalanceResponse.Amount);
            Assert.Equal(
                $"{Config.UsdcAsset.Code}:{Config.UsdcAsset.Issuer}",
                claimableBalanceResponse.Asset
            );
            Assert.Equal(2, claimableBalanceResponse.Claimants.Length);
            Assert.Equal(
                Config.MahekeEscrowPublic,
                claimableBalanceResponse.Claimants[0].Destination
            );
            Assert.Equal(Config.VoterPublic, claimableBalanceResponse.Claimants[1].Destination);
            Assert.Equal(
                proposal.Created.Date.AddDays(32).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                claimableBalanceResponse.Claimants[1].Predicate.Not.AbsBefore
            );
        }
    }
}
