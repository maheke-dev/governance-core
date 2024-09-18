using System;

namespace Maheke.Gov.Infrastructure.Stellar.Exceptions
{
    public abstract class InfrastructureException : ApplicationException
    {
        protected InfrastructureException(string message) : base(message)
        {
        }
    }
}
