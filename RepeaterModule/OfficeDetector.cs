using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace APP {
    /// <summary>
    /// Clase con métodos para la detección de la versión de Office y la instalación de ACE OLEDB
    /// </summary>
    public static class OfficeDetector {
        /// <summary>
        /// Obtiene una lista de los conectores ACE ODBC instalados
        /// </summary>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static List<string> GetInstaledMicrosoftAceOledb() {
            List<string> returnValue = new List<string>();

            try {
                RegistryKey registerKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes");
                string[] registeredClasses = registerKey.GetSubKeyNames();
                string[] installedOledb = registeredClasses.Where(r => r.StartsWith("Microsoft.ACE.OLEDB.")).ToArray();
                returnValue.AddRange(installedOledb);
            } catch { }

            return returnValue;
        }

        /// <summary>
        /// Obtiene la última versión de ACE OLEDB instalada
        /// </summary>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static string GetLastMicrosoftOledb() {
            string returnValue = null;
            try {
                List<string> installedMicrosoftOledb = GetInstaledMicrosoftAceOledb();

                int maxVersion = installedMicrosoftOledb.Max(v => Int32.Parse(v.Replace("Microsoft.ACE.OLEDB.", "").Replace(".0", "")));

                returnValue = $"Microsoft.ACE.OLEDB.{maxVersion}.0";
            } catch { }

            return returnValue;
        }


        /// <summary>
        /// Devuelve el enlace a la versión de Microsoft Access Database Engine sugerida
        /// 
        /// Microsoft Access Database Engine 2007(version 12.0)
        /// Microsoft Access Database Engine 2010(version 14.0)
        /// Microsoft Access Database Engine 2013(version 15.0)
        /// Microsoft Access Database Engine 2016(version 16.0)
        /// Microsoft Access Database Engine 2019(version 16.0)
        /// </summary>
        /// <param name="versionOffice"></param>
        /// <returns></returns>
        public static string GetProposedAccessRuntimeLink(int? versionOffice) {
            string returnValue = @"https://www.microsoft.com/es-ES/download/details.aspx?id=50040";

            if (versionOffice != null)
                switch (versionOffice) {
                    case 12:
                        returnValue = @"https://www.microsoft.com/es-ES/download/details.aspx?id=39358";
                        break;
                    case 14:
                        returnValue = @"https://www.microsoft.com/es-es/download/details.aspx?id=10910";
                        break;
                    case 15:
                        returnValue = @"https://www.microsoft.com/es-ES/download/details.aspx?id=39358";
                        break;
                    case 16:
                        returnValue = @"https://www.microsoft.com/es-ES/download/details.aspx?id=50040";
                        break;
                    default:
                        returnValue = @"https://www.microsoft.com/es-ES/download/details.aspx?id=39358";
                        break;
                }

            return returnValue;
        }

        /// <summary>
        /// Obtiene la versión actual de Office
        /// </summary>
        /// <returns></returns>
        public static int? GetMicrosoftOfficeCurrentVersion() {
            int? returnValue = null;

            try {
                foreach (MicrosoftApplication microsoftApplication in Enum.GetValues(typeof(MicrosoftApplication))) {
                    int? curVer = GetInstaledMicrosoftApplication(microsoftApplication);

                    if (curVer != null) {
                        returnValue = curVer;
                        break;
                    }
                }

            } catch { }

            return returnValue;
        }

        /// <summary>
        /// Obtiene la versión de alguna de las aplicaciones Office
        /// </summary>
        /// <param name="microsoftApplication"></param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        private static int? GetInstaledMicrosoftApplication(MicrosoftApplication microsoftApplication) {
            int? returnValue = null;

            try {
                using (RegistryKey registerKey = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Classes\\{microsoftApplication.ToString()}.Application\\CurVer")) {

                    string value = (string)registerKey.GetValue("");

                    if (value != null) {
                        string[] splittedValue = value.Split('.', StringSplitOptions.None);
                        returnValue = int.Parse(splittedValue[splittedValue.Length - 1]);
                    }
                }

            } catch { }

            return returnValue;
        }
    }



    /// <summary>
    /// Enumerador de productos Office
    /// </summary>
    enum MicrosoftApplication {
        Word,
        Excel,
        Access
    }
}
