using Maheke.Gov.Application.Proposals.Responses;
using Maheke.Gov.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maheke.Gov.Application.Proposals
{
    public interface IProposalRepository
    {
        public Task<Proposal> GetProposal(string assetCode);
        public Task SaveProposal(Proposal proposal);
        public Task<List<ProposalIdentifier>> GetProposalList();
        public Task<int> GetVotingResult(string assetCode);
    }
}
