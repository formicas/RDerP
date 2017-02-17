using System.Globalization;
using System.Windows.Controls;

namespace RDerP
{
    public class MandatoryRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var strValue = value as string;
            if (string.IsNullOrWhiteSpace(strValue))
            {
                return new ValidationResult(false, "Required");
            }
            return new ValidationResult(true, null);
        }
    }
}
