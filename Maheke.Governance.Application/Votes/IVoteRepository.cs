using System.Threading.Tasks;
using Maheke.Gov.Domain;

namespace Maheke.Gov.Application.Votes
{
    public interface IVoteRepository
    {
        public Task<string> GetVoteIntent(ValidatedVote validatedVote, Proposal proposal, string proposalId);
        public Task SaveVote(ValidatedVote validatedVote, Proposal proposal, string proposalId, string voterPrivateKey);
    }
}
