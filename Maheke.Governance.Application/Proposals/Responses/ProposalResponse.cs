using System;
using System.Collections.Generic;
using Maheke.Gov.Application.Options;
using Maheke.Gov.Application.WhitelistedAssets;

namespace Maheke.Gov.Application.Proposals.Responses
{
    public class ProposalResponse : IProposalResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime Created { get; set; }
        public IEnumerable<WhitelistedAssetDto> WhitelistedAssets { get; set; }
        public IEnumerable<OptionDto> Options { get; set; }
        public string VotingResult { get; set; }
    }
}
