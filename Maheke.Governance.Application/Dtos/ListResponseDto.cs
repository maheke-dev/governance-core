using Maheke.Gov.Application.Extensions;
using Maheke.Gov.Application.Proposals.Responses;
using System.Collections.Generic;

namespace Maheke.Gov.Application.Dtos
{
    public class ListResponseDto : ILinkedResource
    {
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<ProposalIdentifier> Items { get; set; }
        public IDictionary<LinkedResourceType, LinkedResource> Links { get; set; }
    }
}
