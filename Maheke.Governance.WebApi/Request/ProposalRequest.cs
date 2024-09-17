using Maheke.Gov.Application.Proposals.Requests;

namespace Maheke.Gov.WebApi.Request
{
    public class ProposalRequest : IProposalRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }
    }
}
