using System.Collections.Generic;

namespace Maheke.Gov.Application.Extensions
{
    public interface ILinkedResource
    {
        public IDictionary<LinkedResourceType, LinkedResource> Links { get; set; }
    }
}
