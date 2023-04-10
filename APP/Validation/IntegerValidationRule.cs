using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace APP.Validation
{
    /// <summary>
    /// Regla de validacion para Integer
    /// </summary>
    public class IntegerValidationRule : ValidationRule
    {
        public int maxRange { get; set; }
        public int minRange { get; set; }
        public bool isRequired { get; set; }
        public bool isPositive { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int result;
            if (String.IsNullOrEmpty((string)value))
            {
                if (isRequired)
                {
                    return new ValidationResult(false, "Este campo debe tener un valor");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }
            if (!int.TryParse((String)value, out result))
            {
                return new ValidationResult(false, "Valor introducido debe ser numérico");
            }
            if (minRange != 0 && result < minRange)
            {
                return new ValidationResult(false, $"No puede ser inferior a {minRange}");
            }
            if (maxRange != 0 && result > maxRange)
            {
                return new ValidationResult(false, $"No puede ser superior a {maxRange}");
            }
            if (isPositive && result < 1)
            {
                return new ValidationResult(false, $"No puede ser inferior a 1");
            }
            return ValidationResult.ValidResult;
        }
    }
}
