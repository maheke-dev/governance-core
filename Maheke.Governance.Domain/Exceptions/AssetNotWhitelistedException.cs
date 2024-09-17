namespace Maheke.Gov.Domain.Exceptions
{
    public class AssetNotWhitelistedException : DomainException
    {
        public AssetNotWhitelistedException(string message) : base(message)
        {
        }
    }
}
