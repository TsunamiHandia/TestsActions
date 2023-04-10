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
    /// Regla de validacion para String
    /// </summary>
    public class StringValidationRule : ValidationRule
    {
        public int maxLength { get; set; }
        public int minLength { get; set; }
        public bool isRequired { get; set; }
        public bool isUrl { get; set; }
        public bool isGuid { get; set; }
        public bool isPath { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {            
            string result = (String)value;
            if (String.IsNullOrEmpty(result))
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
            if (minLength != 0 && result.Length < minLength)
            {
                return new ValidationResult(false, $"No puede ser inferior a {minLength} caracteres");
            }
            if (maxLength != 0 && result.Length > maxLength)
            {
                return new ValidationResult(false, $"No puede ser superior a {maxLength} caracteres");
            }
            if (isUrl && !checkIsUrl(result))
            {
                return new ValidationResult(false, "Formato de URL es incorrecto");
            }
            if (isGuid && !checkIsGuid(result))
            {
                return new ValidationResult(false, "Formato de GUID es incorrecto");
            }
            if (isPath && !checkIsPath(result))
            {
                return new ValidationResult(false, "La ruta es incorrecta");
            }

            return ValidationResult.ValidResult;
        }

        /// <summary>
        /// Comprueba si formato de URL es correcto
        /// </summary>
        /// <param name="url">String con URL</param>
        /// <returns>True/False</returns>
        private bool checkIsUrl(string url)
        {
            Uri uriResult;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult))
            {
                return false;
            }
            if (!(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Comprueba si formato de GUID es correcto
        /// </summary>
        /// <param name="guid">String con GUID</param>
        /// <returns>True/False</returns>
        private bool checkIsGuid(string guid)
        {
            Guid guidResult;                        
            if (!Guid.TryParseExact(guid, "D", out guidResult))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Comprueba si formato de la ruta al directorio es correcto
        /// </summary>
        /// <param name="path">String con la ruta</param>
        /// <returns>True/False</returns>
        private bool checkIsPath(string path)
        {
            try
            {
                System.IO.Path.GetFullPath(path);
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }
    }
}
