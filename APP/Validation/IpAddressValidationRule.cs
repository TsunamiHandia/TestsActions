using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace APP.Validation {
    /// <summary>
    /// Regla de validacion para IP Address
    /// </summary>
    public class IpAddressValidationRule : ValidationRule {
        public bool isRequired { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string result = (String)value;
            if (String.IsNullOrEmpty(result)) {
                if (isRequired)
                    return new ValidationResult(false, "Este campo debe tener un valor");
                else
                    return ValidationResult.ValidResult;
            }
            if (!Regex.IsMatch(result, "^(?:[0-9]{1,3}\\.){3}[0-9]{1,3}$"))
                return new ValidationResult(false, "Este campo debe tener una dirección IP válida");
            else
                return ValidationResult.ValidResult;
        }
    }
}
