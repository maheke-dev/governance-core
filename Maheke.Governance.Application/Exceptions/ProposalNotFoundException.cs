using System;

namespace Maheke.Gov.Application.Exceptions
{
    public class ProposalNotFoundException : MahekeException
    {
        public ProposalNotFoundException(string detail, Exception inner, string title, string type) : base(detail,
            inner, title, "PROPOSAL_NOT_FOUND")
        {
        }
    }
}
