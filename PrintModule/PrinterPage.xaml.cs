using API;
using APP;
using APP.Controller;
using APP.Module;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static API.PrinterController;
using static APP.Controller.AppModulesController;

namespace PrinterModule
{
    /// <summary>
    /// Lógica de interacción para PaginaModuloImpresora.xaml
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class PrinterPage : Page
    {
        public PrinterPage(Guid moduleId)
        {
            InitializeComponent();
            DataContext = new PrinterDataContext(moduleId);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetPrinterList((PrinterPage)sender);
        }

        private async void GetPrinterList(PrinterPage sender)
        {

            Task task = Task.Run(() =>
            {
                List<PrinterDataContext> data = new List<PrinterDataContext>();
                LocalPrinters lclPrinters = new LocalPrinters();
                foreach (var item in PrinterSettings.InstalledPrinters)
                {
                    PrinterDataContext printContext = new PrinterDataContext();
                    printContext.printerName = item.ToString();
                    PrinterSettings printSt = new PrinterSettings();
                    printSt.PrinterName = item.ToString();
                    printContext.color = printSt.SupportsColor;
                    printContext.duplex = printSt.CanDuplex;
                    printContext.isDefault = printSt.IsDefaultPrinter;
                    data.Add(printContext);
                }
                

                this.Dispatcher.Invoke(() =>
                {
                    PrintersDataGrid.ItemsSource = data;
                    WaitLabel.Visibility = Visibility.Collapsed;
                    WaitStatusImage.Visibility = Visibility.Collapsed;
                });
            }
            );

            await task;
        }        
        public static void c_ModuloEventReached(object sender, Dictionary<string, ModuleEvent> e)
        {
            AppModulesController moduleCtrl = new AppModulesController();
            List<AppModules> modulesList = moduleCtrl.List(100, ModuleType.PRINTER);
            foreach (AppModules mod in modulesList)
            {
                ModuleEvent moduleEvent = new ModuleEvent();
                moduleEvent.module = new Module();
                moduleEvent.module.id = (Guid)mod.moduleId;
                moduleEvent.module.name = mod.name;
                moduleEvent.module.type = (ModuleType)mod.type;
                moduleEvent.module.active = (bool)mod.active;
                moduleEvent.module.addedAt = mod.addedAt;
                e.Add(moduleEvent.module.id.ToString(), moduleEvent);
            }
        }

        /// <summary>
        /// Clase de contexto Impresora
        /// </summary>
        public class PrinterDataContext
        {
            public Guid moduleId
            {
                get;
                set;
            }
            public string printerName
            {
                get;
                set;
            }
            public bool color
            {
                get;
                set;
            }
            public bool duplex
            {
                get;
                set;
            }
            public bool isDefault
            {
                get;
                set;
            }

            public string moduleName
            {
                get
                {
                    AppModules module = new AppModulesController().Get(moduleId);
                    if (module == null)
                        return null;

                    return module.name;
                }
                set
                {

                }

            }

            public PrinterDataContext(Guid moduleId)
            {
                this.moduleId = moduleId;
            }
            public PrinterDataContext()
            {
            }
        }

    }
}
