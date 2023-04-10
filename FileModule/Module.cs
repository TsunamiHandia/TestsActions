using APP;
using APP.Controller;
using APP.Module;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace FileModule
{

    public class Module : APP.Module.Module
    {
        private ConfigController configCtrl = new ConfigController();

        public string origen;
        private FileManager FileManager;
        public Module()
        {
            this.minConfigId = 1000;
            this.maxConfigId = 1999;
            this.type = ModuleType.FILE;
        }
        public override ButtonDecor getDecorButton()
        {
            ButtonDecor buttonDecor = new ButtonDecor();
            buttonDecor.Icon = "\uF12B";
            return buttonDecor;
        }

        public override void initDB(SQL sql, bool recreate, bool seed)
        {

            if (String.IsNullOrEmpty(configCtrl.GetValue(FileConfigId.FILE_MODULE_ORIGIN_SOURCE, id)))
            {
                configCtrl.Save(FileConfigId.FILE_MODULE_SICRO_TIME, "60", id);
                configCtrl.Save(FileConfigId.FILE_MODULE_OPTION_MODE, "2", id);
            }
        }

        public override void up()
        {
            ConfigController configCtrl = new ConfigController();
            string origin = configCtrl.GetValue(FileConfigId.FILE_MODULE_ORIGIN_SOURCE, id);
            string destination = configCtrl.GetValue(FileConfigId.FILE_MODULE_DESTINATION_SOURCE, id);
            string searchPattern = configCtrl.GetValue(FileConfigId.FILE_MODULE_SEARCH_PATTERN, id);
            string seconds = configCtrl.GetValue(FileConfigId.FILE_MODULE_SICRO_TIME, id);
            Operacion operation = (Operacion)Enum.Parse(typeof(Operacion), configCtrl.GetValue(FileConfigId.FILE_MODULE_OPTION_MODE, id));
            bool sincroSubFolders = configCtrl.GetValue(FileConfigId.FILE_MODULE_ENABLE_SUBFOLDER_SYNC, id) == "1";

            if (FileManager == null)
            {
                FileManager = new FileManager(origin, destination, operation, sincroSubFolders, id, searchPattern);
                FileManager.Run(seconds);
            }
        }

        public override bool HealthCheck()
        {
            try
            {
                if (!isActiveRemote())
                {
                    new ActivityLogController()
                        .Post(id, ActivityLogController.ActivityLog.Status.ERROR, "Modulo no está configurado en BC");

                    return false;
                }
            }
            catch (Exception ex)
            {
                new ActivityLogController().Post(id, ActivityLogController.ActivityLog.Status.ERROR, ex.Message);
                return false;
            }
            if (FileManager.getDispatcherTimer() == null || !FileManager.getDispatcherTimer().IsEnabled)
            {
                new ActivityLogController()
                    .Post(id, ActivityLogController.ActivityLog.Status.ERROR, "Temporizador no está habilitado");

                return false;
            }

            ActivityLogController.ActivityLog activityLog = new ActivityLogController().List(1, id)
                                                                .Find(a => a.status == ActivityLogController.ActivityLog.Status.ERROR);
            if (activityLog != null)
                return false;

            return true;
        }

        public FileManager getProcess()
        {
            return FileManager;
        }




    }

}
