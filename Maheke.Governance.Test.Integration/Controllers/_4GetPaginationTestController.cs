using Microsoft.AspNetCore.Mvc.Testing;
using Maheke.Gov.Test.Integration.Fixtures;
using Maheke.Gov.Test.Integration.Helpers;
using Maheke.Gov.WebApi;
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
    public class _4GetPaginationTestController : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public _4GetPaginationTestController(
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
        public async Task Test_01_Get_Proposal_List_Paged()
        {
            var httpClient = _factory.CreateClient();

            var limit = 2;
            var page = 3;

            var response = await MahekeHelper.GetList(httpClient, Config, limit, page);

            Assert.Equal(2, response.Items.Count);
            Assert.Equal(4, response.TotalPages);
            Assert.Equal(
                "/Proposal?limit=2&page=2",
                response.Links[Application.Extensions.LinkedResourceType.Prev].Href
            );
            Assert.Equal(
                "/Proposal?limit=2&page=4",
                response.Links[Application.Extensions.LinkedResourceType.Next].Href
            );
        }
    }
}
