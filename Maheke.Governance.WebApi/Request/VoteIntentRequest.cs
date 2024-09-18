using Maheke.Gov.Application.Assets;
using Maheke.Gov.Application.Options;
using Maheke.Gov.Application.Votes.Requests;

namespace Maheke.Gov.WebApi.Request
{
    public class VoteIntentRequest : IVoteIntentRequest
    {
        public decimal Amount { get; set; }
        public AssetDto Asset { get; set; }
        public OptionDto Option { get; set; }
        public string Voter { get; set; }
    }
}
