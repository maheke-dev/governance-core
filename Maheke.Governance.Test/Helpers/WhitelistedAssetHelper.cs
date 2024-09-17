using System.Collections.Generic;
using Maheke.Gov.Domain;

namespace Maheke.Gov.Test.Helpers
{
    public class WhitelistedAssetHelper
    {
        public static IEnumerable<WhitelistedAsset> GetWhitelistedAssets()
        {
            var plt = AssetHelper.GetPlt();
            var ars = AssetHelper.GetArs();
            var usdc = AssetHelper.GetUsdc();
            return new List<WhitelistedAsset> {new(plt, 2m), new(ars, 0.5m), new(usdc, 1m)};
        }
    }
}
