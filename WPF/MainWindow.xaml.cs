using APP.Module;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Diagnostics;
using WPF;
using APP;
using APP.Controller;
using static APP.Controller.AppModulesController;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Hosting;

namespace APP
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class MainWindow : Window
    {
        private Dictionary<Guid, Page> openedPages;

        public MainWindow()
        {
            InitializeComponent();
            openedPages = new Dictionary<Guid, Page>();

            SQL sql = new SQL().open();
            //#if DEBUG
            //    DB.init(sql, true, true);
            //#endif
         
            MainMenu.Children.Clear();
            Module.StoreModules.modulesCollection.Clear();
            ModuleEvents.OnModuloEventReached(Module.StoreModules.modulesCollection);

            /// Para cada modulo activo localmente pintamos el boton, metemos datos iniciales y lanzamos proceso up
            foreach (KeyValuePair<string, ModuleEvent> moduleObj in StoreModules.modulesCollection)
            {
                if (moduleObj.Value.module.active)
                {
                    paintButton(moduleObj.Value.module);
                    //TODO: Llevar al initDB al configurador
                    moduleObj.Value.module.initDB(sql, true, true);
                    moduleObj.Value.module.up();
                }
            }

            /// Navegar a la pagina principal
            RedirectTo(typeof(MainPage), BotonHome);

        }

        /// <summary>
        /// Pinta boton en el menu lateral
        /// </summary>
        /// <param name="module">Objeto del modulo</param>
        private void paintButton(Module.Module module)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.DataContext = module;
            textBlock.MouseUp += BotonMenu_MouseUp;
            textBlock.Style = (Style)Resources["BotonInicio"];
            textBlock.Padding = new Thickness(10, 10, 0, 0);
            textBlock.Text = module.getDecorButton().Icon;
            MainMenu.Children.Add(textBlock);
        }

        /// <summary>
        /// Navega a una pagina en base al id del boton
        /// </summary>
        /// <param name="target">Tipo destinop</param>
        /// <param name="button">Boton del menu lateral</param>
        private void RedirectTo(Type target, TextBlock button)
        {
            ChangeBackgroundButtons();
            button.Background = Brushes.White;
            if (target == typeof(FileModule.FilePage))
            {
                Guid id = ((Module.Module)button.DataContext).id;

                if (SearchPageInMemory(id) == null)
                    this.openedPages.Add(id, new FileModule.FilePage(id));

                this.FrameNavegacion.Navigate(
                    SearchPageInMemory(id)
                );
            }
            if (target == typeof(ScaleModule.ScalePage))
            {
                Guid id = ((Module.Module)button.DataContext).id;

                if (SearchPageInMemory(id) == null)
                    this.openedPages.Add(id, new ScaleModule.ScalePage(id));

                this.FrameNavegacion.Navigate(
                    SearchPageInMemory(id)
                );
            }
            if (target == typeof(FTPModule.FtpPage))
            {
                Guid id = ((Module.Module)button.DataContext).id;

                if (SearchPageInMemory(id) == null)
                    this.openedPages.Add(id, new FTPModule.FtpPage(id));

                this.FrameNavegacion.Navigate(
                    SearchPageInMemory(id)
                );
            }
            else if (target == typeof(PrinterModule.PrinterPage))
            {
                Guid id = ((Module.Module)button.DataContext).id;

                if (SearchPageInMemory(id) == null)
                    this.openedPages.Add(id, new PrinterModule.PrinterPage(id));

                this.FrameNavegacion.Navigate(
                    SearchPageInMemory(id)
                );
            }
            else if (target == typeof(RepeaterModule.RepeaterPage))
            {
                Guid id = ((Module.Module)button.DataContext).id;

                if (SearchPageInMemory(id) == null)
                    this.openedPages.Add(id, new RepeaterModule.RepeaterPage(id));

                this.FrameNavegacion.Navigate(
                    SearchPageInMemory(id)
                );
            }
            else if (target == typeof(MainPage))
            {
                this.FrameNavegacion.Navigate(new Uri("MainPage.xaml", UriKind.Relative));
            }
            else if (target == typeof(ApiPage))
            {
                this.FrameNavegacion.Navigate(new Uri("ApiPage.xaml", UriKind.Relative));
            }
            else if (target == typeof(PaginaRegistroActividad))
            {
                this.FrameNavegacion.Navigate(new Uri("ActivityLogPage.xaml", UriKind.Relative));
            }
            else if (target == typeof(SetupPage))
            {
                this.FrameNavegacion.Navigate(new Uri("SetupPage.xaml", UriKind.Relative));
            }

        }
        [SupportedOSPlatform("windows")]
        private void BotonMenu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock boton = sender as TextBlock;

            switch (((Module.Module)boton.DataContext).type)
            {
                case ModuleType.PRINTER:
                    RedirectTo(typeof(PrinterModule.PrinterPage), (TextBlock)sender);
                    break;
                case ModuleType.FILE:
                    RedirectTo(typeof(FileModule.FilePage), (TextBlock)sender);
                    break;
                case ModuleType.WEIGHING_MACHINE:
                    RedirectTo(typeof(ScaleModule.ScalePage), (TextBlock)sender);
                    break;
                case ModuleType.FTP:
                    RedirectTo(typeof(FTPModule.FtpPage), (TextBlock)sender);
                    break;
                case ModuleType.REPEATER:
                    RedirectTo(typeof(RepeaterModule.RepeaterPage), (TextBlock)sender);
                    break;
                default:
                    // code block
                    break;
            }

        }

        private void BotonConfiguracion_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RedirectTo(typeof(SetupPage), (TextBlock)sender);
        }

        private void BotonImpresora_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Navegar("PaginaModuloImpresora.xaml", (TextBlock)sender);
        }

        private void BotonHome_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RedirectTo(typeof(MainPage), (TextBlock)sender);
        }


        private void BotonRegistroActividad_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RedirectTo(typeof(PaginaRegistroActividad), (TextBlock)sender);
        }



        private void BotonAPI_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RedirectTo(typeof(ApiPage), (TextBlock)sender);
        }

        /// <summary>
        /// Aplica cambios de estilo en todos los botones del panel lateral
        /// </summary>
        private void ChangeBackgroundButtons()
        {
            //reinicio color de fondo a los botones dinamicos
            int count = MainMenu.Children.Count;
            for (int itr = 0; itr < count; itr++)
            {
                if (MainMenu.Children[itr] is TextBlock)
                {
                    ((TextBlock)MainMenu.Children[itr]).Background = null;
                }
            }
            //reinicio color de fondo a los botones estaticos
            BotonConfiguracion.Background = null;
            BotonHome.Background = null;
            BotonAPI.Background = null;
            BotonRegistroActividad.Background = null;

        }

        /// <summary>
        /// Invoca la notificacion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SupportedOSPlatform("windows7.0")]
        private void ShowNotification(object sender, System.ComponentModel.CancelEventArgs e)
        {
          if (IsActive)
            App.ShowNotification("Aplicación en segundo plano", "La aplicación se está ejecutando en segundo plano");
        }

        /// <summary>
        /// Almacena en memoria pagina del modulo
        /// </summary>
        /// <param name="id">Guid del modulo</param>
        /// <returns>Pagina del modulo almacenada en la memoria</returns>
        private Page SearchPageInMemory(Guid id)
        {
            Page searchedPage = this.openedPages.Where(x => x.Key == id)
                                               .Select(x => x.Value)
                                               .FirstOrDefault();
            return searchedPage;
        }

        
    }
}
