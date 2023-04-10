using API;
using APP.Controller;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using WPF;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Input;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace APP
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class App : System.Windows.Application
    {
        private static TaskbarIcon notifyIcon;
        private string clientCertificateName;
        private static Mutex _mutex = null;
        private const string appName = "WIN_TC";
        private bool createdNew;
        private List<string> ports = new List<string>();        
        private static bool startMinimized = false;

        [SupportedOSPlatform("windows7.0")]
        private void Application_Startup(object sender, StartupEventArgs e)
        {            
            /// Kestrel
            ConfigController configCtrl = new ConfigController();
            string servicePort = configCtrl.GetValue(ConfigId.APP_API_PORT);
            ports.Add($"http://+:{servicePort}/");
            bool enableHttps = configCtrl.GetValue(ConfigId.APP_ENABLED_HTTPS) == "1";
            if (enableHttps)
            {
                ports.Clear();
                ports.Add($"https://+:{servicePort}/");             
                clientCertificateName = configCtrl.GetValue(ConfigId.APP_CLIENT_CERTIFICATE);
            }


            if (!string.IsNullOrEmpty(servicePort))                
                KestrelWebApp.Up(ports.ToArray(), clientCertificateName);

            //ruta del ejecutable, de esta forma siempre tengo actualizado el registro hacia el ejecutable indicado
            SetStartup("WIN_TC", Environment.ProcessPath, configCtrl.GetValue(ConfigId.APP_AUTOSTART) == "1");

            //toma los parámetros de entrada
            string [] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--minimized")
                {
                    startMinimized = true;
                }
            }   
            /// Suscrpcion de evento para los Modulos
            ModuleEvents.ModuloEvent += FileModule.FilePage.c_ModuloEventReached;
            ModuleEvents.ModuloEvent += ScaleModule.ScalePage.c_ModuloEventReached;
            ModuleEvents.ModuloEvent += FTPModule.FtpPage.c_ModuloEventReached;
            ModuleEvents.ModuloEvent += PrinterModule.PrinterPage.c_ModuloEventReached;
            ModuleEvents.ModuloEvent += RepeaterModule.RepeaterPage.c_ModuloEventReached;

            if (!string.IsNullOrEmpty(servicePort))
                KestrelWebApp.Up(ports.ToArray(), clientCertificateName);

            Current.MainWindow = NavigateTo();
            if (startMinimized)
            {
                Current.MainWindow.Close();
            }
            else {
                Current.MainWindow.Show();
            }
            
            
        }
        [SupportedOSPlatform("windows7.0")]
        public static Window NavigateTo()
        {
            if (String.IsNullOrEmpty(GetRunCheckError()) && GetConfiguredModules())
            {
                return new MainWindow();
            }
            else
            {
                return new ModulesSetup();
            }
        }

        /// <summary>
        /// Comprueba limites del module
        /// </summary>
        /// <returns>True si hay algun modulo activo</returns>
        public static string GetRunCheckError()
        {
            if (new AppModulesController().List(int.MaxValue).FindAll(x => x.active == true).Count > 8)
                return "No puede haber más de 8 modulos activados";
            if (new AppModulesController().List(int.MaxValue).FindAll(x => x.active == true &&
                                                                      x.type == Module.ModuleType.PRINTER).Count > 1)
                return "No puede haber más de 1 modulo de impresora activado";
            if (new AppModulesController().List(int.MaxValue).FindAll(x => x.active == true &&
                                                                      x.type == Module.ModuleType.REPEATER).Count > 1)
                return "No puede haber más de 1 modulo de repetidor activado";
            if (new AppModulesController().List(int.MaxValue).FindAll(x => x.active == true &&
                                                                      x.type == Module.ModuleType.FTP).Count > 1)
                return "No puede haber más de 1 modulo de FTP activado";
            return default;
        }

        /// <summary>
        /// Compruebo si existen modulos configurados
        /// </summary>
        /// <returns>si no hay salgo completamente</returns>
        public static bool GetConfiguredModules()
        {
            if (new AppModulesController().List(int.MaxValue).Count < 1)
                return false;
            if (!new AppModulesController().List(int.MaxValue).Exists(x => x.active == true))
                return false;
            return true;
        }
        [SupportedOSPlatform("windows7.0")]
        protected override void OnStartup(StartupEventArgs e)
        {
            //control para permitir una instancia única de la aplicacion 
            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
                ShowNotification("Advertencia", "La aplicación ya está ejecutándose en segundo plano");
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                //los siguientes objetos los instanciamos para que se registren las controladoras
                FtpController ftpController = new FtpController();
                PrinterController printerController = new PrinterController();
                ScaleController scaleController = new ScaleController();
                RepeaterController repeaterController = new RepeaterController();                
                if (!SQL.IsDbCreated())
                    DB.init(new SQL().open(), true);
                //creamos el icono (es un recurso dentro MainResourceDictionary.xaml
                notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
                base.OnStartup(e);
            }
        }       

        public static void SetStartup(string AppName,string path, bool enable)
        {            
            // la ruta de la llave donde windows busca las aplicaciones de inicio
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);            

            if (enable)
            {
                startupKey.Close();                   
                startupKey = Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                // ruta del ejecutable 
                string startPath = path + @" --minimized";
                startupKey.SetValue(AppName, startPath);
                startupKey.Close();
            }
            else
            {
                startupKey = Registry.CurrentUser.OpenSubKey(
                                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                startupKey.DeleteValue(AppName, false);
                startupKey.Close();
            }            
        }


        [SupportedOSPlatform("windows7.0")]
        protected override void OnExit(ExitEventArgs e)
        {
            //notifyIcon.Dispose();            
            base.OnExit(e);
        }

        [SupportedOSPlatform("windows7.0")]
        public static void ShowNotification(string title, string text)
        {            
            notifyIcon.Visibility = Visibility.Visible;
            notifyIcon.ShowBalloonTip(title, text, BalloonIcon.Info);
        }

    }
}