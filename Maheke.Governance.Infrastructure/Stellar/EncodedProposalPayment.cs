using System.Collections.Generic;

namespace Maheke.Gov.Infrastructure.Stellar
{
    public class EncodedProposalPayment
    {
        public readonly IList<decimal> EncodedProposalMicropayments;
        public readonly decimal ExcessTokens;

        public EncodedProposalPayment(IList<decimal> encodedProposalMicropayments, decimal excessTokens)
        {
            EncodedProposalMicropayments = encodedProposalMicropayments;
            ExcessTokens = excessTokens;
        }
    }
}
