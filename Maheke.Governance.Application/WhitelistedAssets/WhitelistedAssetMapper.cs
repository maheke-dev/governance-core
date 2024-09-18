using Maheke.Gov.Application.Assets;
using Maheke.Gov.Domain;

namespace Maheke.Gov.Application.WhitelistedAssets
{
    public static class WhitelistedAssetMapper
    {
        public static WhitelistedAssetDto Map(WhitelistedAsset whitelistedAsset)
        {
            return new WhitelistedAssetDto
            {
                Asset = AssetMapper.Map(whitelistedAsset.Asset),
                Multiplier = whitelistedAsset.Multiplier
            };
        }
    }
}
