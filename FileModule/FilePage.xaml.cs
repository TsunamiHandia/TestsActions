using APP;
using APP.Controller;
using APP.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static APP.Controller.AppModulesController;

namespace FileModule
{
    /// <summary>
    /// Lógica de interacción para FilePage.xaml
    /// </summary>
    /// 
    public enum Operacion
    {
        DO_NOTHING = 0,
        DELETE_FILE = 1,
        MOVE_FILE = 2
    }
    public partial class FilePage : Page
    {
        public FilePage(Guid moduleId)
        {
            InitializeComponent();
            DataContext = new FileDataContext(moduleId);
        }

        public static void c_ModuloEventReached(object sender, Dictionary<string, ModuleEvent> e)
        {
            AppModulesController moduleCtrl = new AppModulesController();
            List<AppModules> modulesList = moduleCtrl.List(int.MaxValue)
                                                        .Where(a => a.type == ModuleType.FILE).ToList();
            foreach (AppModules mod in modulesList)
            {
                ModuleEvent moduleEvent = new ModuleEvent();
                moduleEvent.module = new Module();
                moduleEvent.module.id = (Guid)mod.moduleId;
                moduleEvent.module.name = mod.name;
                moduleEvent.module.type = (ModuleType)mod.type;
                moduleEvent.module.active = (bool)mod.active;
                moduleEvent.module.registerLog = (bool)mod.registerLog;
                moduleEvent.module.addedAt = mod.addedAt;
                e.Add(moduleEvent.module.id.ToString(), moduleEvent);
            }
        }
    }


    /// <summary>
    /// Clase de contexto Fichero
    /// </summary>
    public class FileDataContext
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

        public string originSource
        {
            get
            {
                return configCtrl.GetValue(FileConfigId.FILE_MODULE_ORIGIN_SOURCE, moduleId);
            }
            set
            {
                configCtrl.Save(FileConfigId.FILE_MODULE_ORIGIN_SOURCE, value, moduleId);
                ModuleEvent moduleRef;
                StoreModules.modulesCollection.TryGetValue(moduleId.ToString(), out moduleRef);
                ((Module)moduleRef.module).getProcess().setOrigin(value);
            }
        }

        public string searchPattern
        {
            get
            {
                return configCtrl.GetValue(FileConfigId.FILE_MODULE_SEARCH_PATTERN, moduleId);
            }
            set
            {
                configCtrl.Save(FileConfigId.FILE_MODULE_SEARCH_PATTERN, value, moduleId);
                ModuleEvent moduleRef;
                StoreModules.modulesCollection.TryGetValue(moduleId.ToString(), out moduleRef);
                ((Module)moduleRef.module).getProcess().setSearchPattern(value);
            }
        }
        public string destinationSource
        {
            get
            {
                return configCtrl.GetValue(FileConfigId.FILE_MODULE_DESTINATION_SOURCE, moduleId);
            }
            set
            {
                configCtrl.Save(FileConfigId.FILE_MODULE_DESTINATION_SOURCE, value, moduleId);
                ModuleEvent moduleRef;
                StoreModules.modulesCollection.TryGetValue(moduleId.ToString(), out moduleRef);
                ((Module)moduleRef.module).getProcess().setDestination(value);
            }
        }

        private bool _typeMoveFile;
        public bool typeMoveFile
        {
            get
            {
                _typeMoveFile = (Operacion)Enum.Parse(typeof(Operacion), configCtrl.GetValue(FileConfigId.FILE_MODULE_OPTION_MODE, moduleId)) == Operacion.MOVE_FILE;
                return _typeMoveFile;
            }
            set
            {
                _typeMoveFile = value;
                if (_typeMoveFile)
                {
                    configCtrl.Save(FileConfigId.FILE_MODULE_OPTION_MODE, Operacion.MOVE_FILE.ToString(), moduleId);
                    ModuleEvent moduleRef;
                    StoreModules.modulesCollection.TryGetValue(moduleId.ToString(), out moduleRef);
                    ((Module)moduleRef.module).getProcess().setOperation(Operacion.MOVE_FILE);
                    this.typeDeleteFile = false;
                }
            }
        }

        private bool _typeDeleteFile;
        public bool typeDeleteFile
        {
            get
            {
                _typeDeleteFile = (Operacion)Enum.Parse(typeof(Operacion), configCtrl.GetValue(FileConfigId.FILE_MODULE_OPTION_MODE, moduleId)) == Operacion.DELETE_FILE;
                return _typeDeleteFile;
            }
            set
            {
                _typeDeleteFile = value;
                if (_typeDeleteFile)
                {
                    configCtrl.Save(FileConfigId.FILE_MODULE_OPTION_MODE, Operacion.DELETE_FILE.ToString(), moduleId);
                    ModuleEvent moduleRef;
                    StoreModules.modulesCollection.TryGetValue(moduleId.ToString(), out moduleRef);
                    ((Module)moduleRef.module).getProcess().setOperation(Operacion.DELETE_FILE);
                    this.typeMoveFile = false;
                }
            }
        }

        private bool _EnableSubFolderSync;
        public bool EnableSubFolderSync {
            get {
                _EnableSubFolderSync = configCtrl.GetValue(FileConfigId.FILE_MODULE_ENABLE_SUBFOLDER_SYNC, moduleId) == "1";
                return _EnableSubFolderSync;
            }
            set {
                _EnableSubFolderSync = value;
                configCtrl.Save(FileConfigId.FILE_MODULE_ENABLE_SUBFOLDER_SYNC, value ? "1" : "0", moduleId);
                ModuleEvent moduleRef;
                StoreModules.modulesCollection.TryGetValue(moduleId.ToString(), out moduleRef);
                ((Module)moduleRef.module).getProcess().setSincroSubFolders(value);
            }
        }

        public string sincroTime
        {
            get
            {
                return configCtrl.GetValue(FileConfigId.FILE_MODULE_SICRO_TIME, moduleId);
            }
            set
            {
                configCtrl.Save(FileConfigId.FILE_MODULE_SICRO_TIME, value, moduleId);
                int seconds;
                int.TryParse(value, out seconds);
                ModuleEvent moduleRef;
                StoreModules.modulesCollection.TryGetValue(moduleId.ToString(), out moduleRef);
                ((Module)moduleRef.module).getProcess().getDispatcherTimer().Interval = new TimeSpan(0, 0, seconds);
            }
        }

        private ConfigController configCtrl;
        public FileDataContext(Guid moduleId)
        {
            configCtrl = new ConfigController();
            this.moduleId = moduleId;
        }
    }

}
