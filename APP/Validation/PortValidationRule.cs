using APP.Controller;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace APP.Validation
{
    public class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult returnValue = ValidationResult.ValidResult;

            if (!CheckPort((string)value))
                returnValue = new ValidationResult(false, $"El puerto {(string)value}, ya está siendo usado.");

            return returnValue;
        }


        /// <summary>
        /// Comprueba si el nuevo puerto está libre
        /// </summary>
        /// <param name="newPort"></param>
        /// <returns></returns>
        private bool CheckPort(string newPort)
        {
            bool returnValue = false;

            ConfigController configCtrl = new ConfigController();
            string currentAPIPort = configCtrl.GetValue(ConfigId.APP_API_PORT);

            if (!newPort.Equals(currentAPIPort))
            {
                int port = Int32.Parse(newPort);
                bool isAvailable = true;

                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                var usedPorts = Enumerable.Empty<int>()
                    .Concat(ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port))
                    .Concat(ipGlobalProperties.GetActiveTcpListeners().Select(l => l.Port))
                    .Concat(ipGlobalProperties.GetActiveUdpListeners().Select(l => l.Port))
                    .ToHashSet();

                foreach (int ipEndPoint in usedPorts)
                {
                    if (ipEndPoint == port)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                    returnValue = true;
            }
            else
                returnValue = true;

            return returnValue;
        }
    }
}
