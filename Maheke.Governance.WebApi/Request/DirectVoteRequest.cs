using Maheke.Gov.Application.Assets;
using Maheke.Gov.Application.Options;
using Maheke.Gov.Application.Votes.Requests;

namespace Maheke.Gov.WebApi.Request
{
    public class DirectVoteRequest : IDirectVoteRequest
    {
        public decimal Amount { get; set; }
        public AssetDto Asset { get; set; }
        public OptionDto Option { get; set; }
        public string Voter { get; set; }
        public string PrivateKey { get; set; }
    }
}
