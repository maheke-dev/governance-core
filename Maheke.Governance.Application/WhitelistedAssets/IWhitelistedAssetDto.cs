using Maheke.Gov.Application.Assets;

namespace Maheke.Gov.Application.WhitelistedAssets
{
    public interface IWhitelistedAssetDto
    {
        public AssetDto Asset { get; set; }
        public decimal Multiplier { get; set; }
    }
}
