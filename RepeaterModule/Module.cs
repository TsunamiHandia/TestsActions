using APP;
using APP.Controller;
using APP.Module;
using System;

namespace RepeaterModule
{
    public class Module : APP.Module.Module
    {
        public Module()
        {
            this.minConfigId = 4000;
            this.maxConfigId = 4999;
            this.name = "Repeater Module";
            this.type = ModuleType.REPEATER;
        }
        public override ButtonDecor getDecorButton()
        {
            ButtonDecor buttonDecor = new ButtonDecor();
            buttonDecor.Icon = "\uE95A";
            return buttonDecor;
        }

        public override void initDB(SQL sql, bool recreate, bool seed)
        {

        }

        public override void up()
        {

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
    }
}
