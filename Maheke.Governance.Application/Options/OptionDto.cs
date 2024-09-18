using Maheke.Gov.Domain;

namespace Maheke.Gov.Application.Options
{
    public class OptionDto : IOptionDto
    {
        public string Name { get; set; }
    
        public static explicit operator Option(OptionDto option)
        {
            return new Option(option.Name);
        }

        public static explicit operator OptionDto(Option option)
        {
            return new OptionDto{Name = option.Name};
        }
    }
}
