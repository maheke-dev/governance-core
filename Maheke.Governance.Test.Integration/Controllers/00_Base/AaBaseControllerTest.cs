using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Maheke.Gov.Test.Integration.Fixtures;
using Maheke.Gov.Test.Integration.Helpers;
using Maheke.Gov.WebApi;
using Xunit;
using Xunit.Abstractions;

namespace Maheke.Gov.Test.Integration.Controllers._00_Base
{
    [Collection("Stellar collection")]
    [TestCaseOrderer("Maheke.Gov.Test.Integration.Helpers.AlphabeticalOrderer", "Maheke.Gov.Test.Integration")]
    public class AaBaseControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public AaBaseControllerTest(
            WebApplicationFactory<Program> factory,
            StellarFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _factory = factory;
            _testOutputHelper = testOutputHelper;
            Config = fixture.Config;
        }

        private TestConfiguration Config { get; }

        [Fact]
        public async Task Test_0_SETUP()
        {
            await StellarHelper.AddXlmFunds(Config.MahekeMicropaymentSenderKeyPair);
            await StellarHelper.AddXlmFunds(Config.MahekeMicropaymentReceiverKeyPair);
            await StellarHelper.AddXlmFunds(Config.MahekeEscrowKeyPair);
            await StellarHelper.AddXlmFunds(Config.ProposalCreator1KeyPair);
            await StellarHelper.AddXlmFunds(Config.ProposalCreator2KeyPair);
            await StellarHelper.AddXlmFunds(Config.VoterKeyPair);
            Assert.True(true);
        }
    }
}
