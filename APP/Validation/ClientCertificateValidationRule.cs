using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;

namespace APP.Validation {
    public class ClientCertificateValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            ValidationResult returnValue = ValidationResult.ValidResult;
            
            if (!GetCertificateFromPersonalStore((string)value))
                returnValue = new ValidationResult(false, $"El certificado {(string)value}, no es válido.");

            return returnValue;
        }

        public static bool GetCertificateFromPersonalStore(string thumbprint)
        {
            var store = GetPersonalCertificates();
            X509Certificate2 certificate = null;
            foreach (var cert in store.Cast<X509Certificate2>().Where(cert => cert.Thumbprint.Equals(thumbprint.ToUpper())))
            {
                certificate = cert;
            }
            return certificate != null;
        }

        private static X509Certificate2Collection GetPersonalCertificates()
        {
            var localMachineStore = new X509Store(StoreLocation.CurrentUser);
            localMachineStore.Open(OpenFlags.ReadOnly);
            var certificates = localMachineStore.Certificates;
            localMachineStore.Close();
            return certificates;
        }

    }      
}
