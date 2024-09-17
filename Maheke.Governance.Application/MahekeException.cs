using System;

namespace Maheke.Gov.Application
{
    public class MahekeException : ApplicationException
    {
        public MahekeException(string detail, Exception inner, string title, string type) : base(detail, inner)
        {
            Title = title;
            Type = type;
        }

        public string Title { get; }
        public string Type { get; }
    }
}
