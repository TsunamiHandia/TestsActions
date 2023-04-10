using API;
using APP;
using APP.Controller;
using APP.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static APP.Controller.AppModulesController;
using static RepeaterModule.Enums;

namespace RepeaterModule {

    /// <summary>
    /// Interaction logic for RepeaterPage.xaml
    /// </summary>
    public partial class RepeaterPage : Page
    {       
        public RepeaterPage(Guid moduleId) {
            InitializeComponent();
            DataContext = new RepeaterDataContext(moduleId);
        }
      
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            GetAceOldbList((RepeaterPage)sender);
        }

        private async void GetAceOldbList(RepeaterPage sender) {

            RepeaterDataContext repeaterDataContext = (RepeaterDataContext) this.DataContext;

            Task task = Task.Run(() => {
                List<RepeaterDataContext> data = new List<RepeaterDataContext>();
                InstalledMicrosoftAceOledb microsoftAceOledb = new InstalledMicrosoftAceOledb();

                List<string> installedMicrosoftAceOledb = OfficeDetector.GetInstaledMicrosoftAceOledb();

                foreach (var aceOledb in installedMicrosoftAceOledb) {
                    RepeaterDataContext repeaterDataContext = new RepeaterDataContext();
                    repeaterDataContext.aceOledb = aceOledb;
                    data.Add(repeaterDataContext);
                }

                int? versionOffice = OfficeDetector.GetMicrosoftOfficeCurrentVersion();
                string proposedMicrosoftAccessRuntime = OfficeDetector.GetProposedAccessRuntimeLink(versionOffice);
                repeaterDataContext.aceOledbCurVer = OfficeDetector.GetLastMicrosoftOledb(); 

                this.Dispatcher.Invoke(() => {
                    OleDbDataGrid.ItemsSource = data;
                    WaitLabel.Visibility = Visibility.Collapsed;
                    WaitStatusImage.Visibility = Visibility.Collapsed;

                    if (string.IsNullOrEmpty(repeaterDataContext.aceOledbCurVer) && !string.IsNullOrEmpty(proposedMicrosoftAccessRuntime)) {
                        MensajeRichTextBox.Visibility = Visibility.Visible;
                        AccessRuntimeLink.Visibility = Visibility.Visible;

                        FlowDocument myFlowDoc = new FlowDocument();

                        myFlowDoc.Blocks.Add(new Paragraph(new Bold(new Run("Ayuda a la instalación Microsoft Access Database Engine"))));
                        myFlowDoc.Blocks.Add(new Paragraph(new Run("No tiene instala ninguna versión de Microsoft Access Database Engine, si su repetidor ha de conectarse a una base de datos Microsoft Access, debe instalar uno.")));                        
                      
                        myFlowDoc.Blocks.Add(new Paragraph(
                            new Run(
                                versionOffice != null ?
                                    "Dada la versión de Office existente en su equipo, debe instalar la versión de Microsoft Access Database Engine que puede encontrar en el enlace que se encuentra más abajo. " :
                                    "Usted no tiene instalada ninguna versión de Office, , debe instalar la versión de Microsoft Access Database Engine que puede encontrar en el enlace que se encuentra más abajo. ")
                                )
                            );

                        myFlowDoc.Blocks.Add(new Paragraph(new Bold(new Run("NOTA:"))));
                        myFlowDoc.Blocks.Add(new Paragraph(new Run("La versión de Microsoft Access Database Engine que se ha de instalar, debe ser x64")));

                        accessRuntimeURL.NavigateUri = new Uri(proposedMicrosoftAccessRuntime);

                        MensajeRichTextBox.Document = myFlowDoc;
                    }
                });
            }
            );

            await task;
        }


        public static void c_ModuloEventReached(object sender, Dictionary<string, ModuleEvent> e) {
            AppModulesController moduleCtrl = new AppModulesController();
            List<AppModules> modulesList = moduleCtrl.List(100, ModuleType.REPEATER);
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
        /// Clase de contexto Repetidor
        /// </summary>
        public class RepeaterDataContext
        {
            public Guid moduleId
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

            public string aceOledb { get; set; }

            public string aceOledbCurVer {
                get {
                    return configCtrl.GetValue(RepeaterConfigId.REPEATER_MODULE_ACE_OLEDB_VERSION, moduleId);
                }
                set {
                    configCtrl.Save(RepeaterConfigId.REPEATER_MODULE_ACE_OLEDB_VERSION, value, moduleId);
                }
            }

            private ConfigController configCtrl;

            public RepeaterDataContext(Guid moduleId)
            {
                this.moduleId = moduleId;
                configCtrl = new ConfigController();
            }
            public RepeaterDataContext() {
                configCtrl = new ConfigController();
            }
                        
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            //Process.Start(((Hyperlink)sender).NavigateUri.ToString());
            if (Uri.TryCreate(e.Uri.ToString(), UriKind.Absolute, out Uri uri)) {
                _ = Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
        }
    }
}
