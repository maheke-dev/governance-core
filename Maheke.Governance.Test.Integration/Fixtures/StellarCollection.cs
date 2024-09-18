using Xunit;

namespace Maheke.Gov.Test.Integration.Fixtures
{
    [CollectionDefinition("Stellar collection")]
    public class StellarCollection : ICollectionFixture<StellarFixture>
    {
    }
}
