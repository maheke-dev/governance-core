using Maheke.Gov.Application.Assets;
using Maheke.Gov.Application.Options;

namespace Maheke.Gov.Application.Votes.Requests
{
    public interface IVoteIntentRequest
    {
        public decimal Amount { get; set; }
        public AssetDto Asset { get; set; }
        public OptionDto Option { get; set; }
        public string Voter { get; set; }
    }
}
