using APP;
using APP.Controller;
using APP.Module;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static APP.Controller.AppModulesController;

namespace FTPModule
{
    /// <summary>
    /// Lógica de interacción para FtpPage.xaml
    /// </summary>
    public partial class FtpPage : Page
    {
        public FtpPage(Guid moduleId)
        {
            InitializeComponent();
            DataContext = new FtpDataContext(moduleId);
        }
        public static void c_ModuloEventReached(object sender, Dictionary<string, ModuleEvent> e)
        {
            AppModulesController moduleCtrl = new AppModulesController();
            List<AppModules> modulesList = moduleCtrl.List(100, ModuleType.FTP);
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
        /// Clase de contexto Fichero
        /// </summary>
        public class FtpDataContext
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

            public FtpDataContext(Guid moduleId)
            {
                this.moduleId = moduleId;
            }
        }

    }
}
