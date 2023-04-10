using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using APP;
using APP.Controller;
using APP.Module;
using API;
using System.Net.Http;
using System.Drawing.Printing;

namespace FTPModule
{
    public class Module : APP.Module.Module
    {
        private FtpManager ftpManager;

        public Module()
        {
            this.minConfigId = 5000;
            this.maxConfigId = 5999;
            this.name = "Ftp Module";
            this.type = ModuleType.FTP;

        }

        public override ButtonDecor getDecorButton()
        {
            ButtonDecor buttonDecor = new ButtonDecor();
            buttonDecor.Icon = "\uE71D";
            return buttonDecor;
        }

        public override void initDB(SQL sql, bool recreate, bool seed)
        {

        }

        public override void up()
        {
            this.ftpManager = new FtpManager(id);
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
            catch(Exception ex)
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

        public FtpManager GetFtpManager()
        {
            return ftpManager;
        }
    }
}
