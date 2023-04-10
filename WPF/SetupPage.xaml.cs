using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using API;
using APP;
using APP.Controller;
using APP.Module;
using Microsoft.AspNetCore.Hosting;
using ScaleModule;

namespace WPF
{
    /// <summary>
    /// Lógica de interacción para Configuracion.xaml
    /// </summary>
    public partial class SetupPage : Page
    {
        public SetupPage()
        {
            InitializeComponent();
        }

        private void ShowModulesSetup(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = new ModulesSetup();
                Application.Current.MainWindow.Show();
            }
        }
    }

    /// <summary>
    /// Clase de contexto Config
    /// </summary>
    public class ConfigDataContext
    {

        private List<string> oldUrls = new List<string>();
        private List<string> newUrls = new List<string>();
        private string clientCertificateName;
        public string apiPort
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_API_PORT);
            }
            set
            {
                newUrls.Clear();
                newUrls.Add($"http://+:{value}/");
                if (configCtrl.GetValue(ConfigId.APP_ENABLED_HTTPS) == "1")
                {
                    newUrls.Clear();
                    newUrls.Add($"https://+:{value}/");
                }
                if (RestartKestrel(newUrls))
                {
                    configCtrl.Save(ConfigId.APP_API_PORT, value);
                    oldUrls.Clear();
                    oldUrls.AddRange(newUrls);
                }
            }
        }

        private bool _EnableHttpsPort;
        public bool EnableHttpsPort
        {
            get
            {
                _EnableHttpsPort = configCtrl.GetValue(ConfigId.APP_ENABLED_HTTPS) == "1";
                return _EnableHttpsPort;
            }
            set
            {
                _EnableHttpsPort = value;

                if (_EnableHttpsPort)
                {
                    newUrls.Clear();
                    newUrls.Add($"https://+:{configCtrl.GetValue(ConfigId.APP_API_PORT)}/");
                    clientCertificateName = configCtrl.GetValue(ConfigId.APP_CLIENT_CERTIFICATE);
                    if (RestartKestrel(newUrls, clientCertificateName))
                    {
                        configCtrl.Save(ConfigId.APP_ENABLED_HTTPS, "1");
                        oldUrls.Clear();
                        oldUrls.AddRange(newUrls);
                    }
                }
                else
                {
                    newUrls.Clear();
                    newUrls.Add($"http://+:{configCtrl.GetValue(ConfigId.APP_API_PORT)}/");
                    if (RestartKestrel(newUrls))
                    {
                        configCtrl.Save(ConfigId.APP_ENABLED_HTTPS, "0");
                        oldUrls.Clear();
                        oldUrls.AddRange(newUrls);
                    }
                }
            }
        }

        public string clientCertificate
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_CLIENT_CERTIFICATE);
            }
            set
            {
                newUrls.Clear();
                newUrls.Add($"https://+:{configCtrl.GetValue(ConfigId.APP_API_PORT)}/");
                clientCertificateName = configCtrl.GetValue(ConfigId.APP_CLIENT_CERTIFICATE);
                if (RestartKestrel(newUrls, clientCertificateName))
                {
                    configCtrl.Save(ConfigId.APP_CLIENT_CERTIFICATE, value);
                    oldUrls.Clear();
                    oldUrls.AddRange(newUrls);
                }
            }
        }

        /// <summary>
        /// Reinicia el kestrel con el nuevo puerto. Si no puede, intenta restablecer el servidor con el puerto anterior.
        /// </summary>
        /// <param name="newPort"></param>
        /// <returns></returns>
        private bool RestartKestrel(List<string> newPorts, string clientCertificateName = null)
        {
            bool returnValue = false;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            ActivityLogController activityLogCtrl = new ActivityLogController();

            Guid IdModule = default;

            try
            {
                KestrelWebApp.Down();
                KestrelWebApp.Up(newPorts.ToArray(), clientCertificateName);
                returnValue = true;
                foreach (var port in newPorts)
                {
                    activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.OK, $"Se ha configurado el puerto {port} correctamente.");
                }
            }
            catch (Exception ex)
            {
                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, $"Se ha producido un error intentado cambiar puerto.\nSe reestablecerá la configuración anterior.\nError: {ex.Message}");

                try
                {
                    KestrelWebApp.Down();
                    KestrelWebApp.Up(oldUrls.ToArray(), clientCertificateName);
                    returnValue = true;
                    activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.OK, $"Se ha restablecido la configuración anterior, puerto: {configCtrl.GetValue(ConfigId.APP_API_PORT)}");
                }
                catch (Exception exception)
                {
                    activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.ERROR, $"Se ha producido un error intentado restablecer el puerto {configCtrl.GetValue(ConfigId.APP_API_PORT)}.\nError: {exception.Message}");
                }

            }

            return returnValue;
        }
        public string apiKey
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_API_KEY);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_API_KEY, value);
            }
        }

        private bool _authTypeCloud;
        public bool authTypeCloud
        {
            get
            {
                _authTypeCloud = (EnvType)Enum.Parse(typeof(EnvType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE)) == EnvType.CLOUD;
                return _authTypeCloud;
            }
            set
            {
                _authTypeCloud = value;
                if (_authTypeCloud)
                {
                    configCtrl.Save(ConfigId.APP_AUTH_TYPE, EnvType.CLOUD.ToString());
                    this.authTypeOnPrem = false;
                }
            }
        }

        private bool _authTypeOnPrem;
        public bool authTypeOnPrem
        {
            get
            {
                _authTypeOnPrem = (EnvType)Enum.Parse(typeof(EnvType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE)) == EnvType.ONPREMISE;
                return _authTypeOnPrem;
            }
            set
            {
                _authTypeOnPrem = value;
                if (_authTypeOnPrem)
                {
                    configCtrl.Save(ConfigId.APP_AUTH_TYPE, EnvType.ONPREMISE.ToString());
                    this.authTypeCloud = false;
                }
            }
        }

        private bool _basicAuthTypeOnPrem;
        public bool basicAuthTypeOnPrem
        {
            get
            {
                _basicAuthTypeOnPrem = (AuthType)Enum.Parse(typeof(AuthType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE_HEADER)) == AuthType.BASIC;
                return _basicAuthTypeOnPrem;
            }
            set
            {
                _basicAuthTypeOnPrem = value;
                if (_basicAuthTypeOnPrem)
                {
                    configCtrl.Save(ConfigId.APP_AUTH_TYPE_HEADER, AuthType.BASIC.ToString());
                    this.NtlmAuthTypeOnPrem = false;
                }
            }
        }

        private bool _NtlmAuthTypeOnPrem;
        public bool NtlmAuthTypeOnPrem
        {
            get
            {
                _NtlmAuthTypeOnPrem = (AuthType)Enum.Parse(typeof(AuthType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE_HEADER)) == AuthType.NTLM;
                return _NtlmAuthTypeOnPrem;
            }
            set
            {
                _NtlmAuthTypeOnPrem = value;
                if (_NtlmAuthTypeOnPrem)
                {
                    configCtrl.Save(ConfigId.APP_AUTH_TYPE_HEADER, AuthType.NTLM.ToString());
                    this.basicAuthTypeOnPrem = false;
                }
            }
        }

        public string onPremiseUrl
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_URL);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_AUTH_ONPREMISE_URL, value);
            }
        }

        public string onPremiseUser
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_USER);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_AUTH_ONPREMISE_USER, value);
            }
        }

        public string onPremisePassword
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_PASSWORD);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_AUTH_ONPREMISE_PASSWORD, value);
            }
        }

        public string tenantId
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_TENANTID);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_AUTH_TENANTID, value);
            }
        }

        public string clientId
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_CLIENTID);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_AUTH_CLIENTID, value);
            }
        }

        public string clientSecret
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_CLIENTSECRET);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_AUTH_CLIENTSECRET, value);
            }
        }

        public string environment
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_ENVIRONMENT);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_AUTH_ENVIRONMENT, value);
            }
        }

        public string id
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_ID);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_ID, value);
            }
        }

        public string companyId
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_COMPANY_ID);
            }
            set
            {
                configCtrl.Save(ConfigId.APP_COMPANY_ID, value);
            }
        }

        private bool _RegisterAutoStartStatus;
        public bool RegisterAutoStartStatus
        {
            get
            {
                _RegisterAutoStartStatus = configCtrl.GetValue(ConfigId.APP_AUTOSTART) == "1";
                return _RegisterAutoStartStatus;
            }
            set
            {
                _RegisterAutoStartStatus = value;
                if (_RegisterAutoStartStatus)
                {
                    configCtrl.Save(ConfigId.APP_AUTOSTART, "1");
                }
                else
                {
                    configCtrl.Save(ConfigId.APP_AUTOSTART, "0");
                }
                //se modifica el registro para hacer efectivo el cambio
                App.SetStartup("WIN_TC", Environment.ProcessPath, _RegisterAutoStartStatus);
            }
        }

        public string Password
        {
            get
            {
                return configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_PASSWORD);
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    configCtrl.Save(ConfigId.APP_AUTH_ONPREMISE_PASSWORD, value);
                }

            }
        }
        private ConfigController configCtrl;
        public ConfigDataContext()
        {
            configCtrl = new ConfigController();
        }
    }
}
