using APP;
using APP.Controller;
using APP.Module;
using System;
using System.Drawing.Printing;
using System.Runtime.Versioning;

namespace PrinterModule
{
    [SupportedOSPlatform("windows")]
    public class Module : APP.Module.Module
    {   
        private PrintManager printManager;

        public Module()
        {
            this.minConfigId = 3000;
            this.maxConfigId = 3999;
            this.type = ModuleType.PRINTER;
        }
        public override ButtonDecor getDecorButton()
        {
            ButtonDecor buttonDecor = new ButtonDecor();
            buttonDecor.Icon = "\uE749";
            return buttonDecor;
        }

        public override void initDB(SQL sql, bool recreate, bool seed)
        {
        }

        public override void up()
        {
            this.printManager = new PrintManager(id);
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

            ActivityLogController.ActivityLog activityLog = new ActivityLogController().List(1, id)
                                                                .Find(a => a.status == ActivityLogController.ActivityLog.Status.ERROR);
            if (activityLog != null)
                return false;

            return true;
        }      

        public PrintManager GetPrintManager()
        {
            return printManager;
        }

    }

}
