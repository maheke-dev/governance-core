using System;

namespace Maheke.Gov.Domain.Exceptions
{
    public abstract class DomainException : ApplicationException
    {
        protected DomainException(string message) : base(message)
        {
        }
    }
}
