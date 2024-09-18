namespace Maheke.Gov.Domain.Exceptions
{
    public class DeadlinePassedException : DomainException
    {
        public DeadlinePassedException(string message) : base(message)
        {
        }
    }
}
