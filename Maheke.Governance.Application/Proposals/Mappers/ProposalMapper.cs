using System.Linq;
using Maheke.Gov.Application.Options;
using Maheke.Gov.Application.Proposals.Responses;
using Maheke.Gov.Application.WhitelistedAssets;
using Maheke.Gov.Domain;

namespace Maheke.Gov.Application.Proposals.Mappers
{
    public static class ProposalMapper
    {
        public static IProposalResponse Map(Proposal proposal, string votingResult)
        {
            return new ProposalResponse
            {
                Name = proposal.Name,
                Description = proposal.Description,
                Creator = proposal.Creator,
                Deadline = proposal.Deadline,
                Created = proposal.Created,
                WhitelistedAssets = proposal.WhitelistedAssets.Select(WhitelistedAssetMapper.Map),
                Options = proposal.Options.Select(option => (OptionDto)option),
                VotingResult = votingResult
            };
        }
    }
}
