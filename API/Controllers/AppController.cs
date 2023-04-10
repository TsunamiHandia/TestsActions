using APP.Controller;
using APP.Module;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using static API.AppController;
using static APP.Controller.AppModulesController;

namespace API {
   
    /// <summary>
    /// lista los modulos en la app
    /// </summary>
    [ApiController]    
    public class AppController : ControllerBase
    {

        private static string message = "{0} al {1} {2} {3} en Business Central";

        [Route("/api/app")]
        [HttpGet]
        public IActionResult Get()
        {
            ConfigController configController = new ConfigController();

            InfoClass infoClass = new InfoClass();
            infoClass.id = configController.GetValue(ConfigId.APP_ID);
            infoClass.modules = new LinkedList<ModuleClass>();

            AppModulesController appModulesController = new AppModulesController();

            foreach (AppModules appModule in appModulesController.List(int.MaxValue)) {
                ModuleClass moduleClass = new ModuleClass() {
                    id = appModule.moduleId.ToString(),
                    type = (int) appModule.type,
                    name = appModule.name,
                    active = appModule.active,
                    addedAt = appModule.addedAt,
                };

                infoClass.modules.AddLast(moduleClass);
            }

            return Ok(infoClass);
        }


        /// <summary>
        /// Notifica que un modulo se ha configurado en el remoto
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("/api/app/setup")]
        [HttpPost]
        public object SetModuleConfig([FromBody] ModuleInfo value)
        {
            ActivityLogController activityLogCtrl = new ActivityLogController();
            Module module = StoreModules.GetModule(value.moduleId);
            if (module == null) {                
                throw new Exception("No existe modulo");
            }
            String msg = String.Format(message, "Éxito", "configurar", "módulo", module.name);
            activityLogCtrl.Post(module.id, ActivityLogController.ActivityLog.Status.OK, msg);

            return new { message = msg };

         }

        public class InfoClass
        {            
            /// <summary>
            /// GUID del terminal
            /// </summary>
            public string id;
            /// <summary>
            /// Lista de los módulos instalados
            /// </summary>
            public LinkedList<ModuleClass> modules = new LinkedList<ModuleClass>();
        }

        public class ModuleInfo
        {
            /// <summary>
            /// GUID del modulo
            /// </summary>
            public Guid moduleId;
        }

        public class ModuleClass 
        {
            /// <summary>
            /// GUID del módulo
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// Enumerado ModuleType
            /// </summary>
            public int type { get; set; }
            /// <summary>
            /// Nombre del módulo
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Estado del módulo
            /// </summary>
            public bool? active { get; set; }
            /// <summary>
            /// Fecha alta módulo
            /// </summary>
            public DateTime? addedAt { get; set; }
        }
    }
}
