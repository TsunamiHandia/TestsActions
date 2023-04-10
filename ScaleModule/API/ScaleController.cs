using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APP;
using APP.Controller;
using ScaleModule;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace API
{
    [ApiController]    
    public class ScaleController : ControllerBase
    {
        [Route("/api/scale")]
        [HttpPost]
        public IActionResult Post([FromBody] ScaleModel value)
        {
            ScaleClass scale = new ScaleClass();

            Module module = (Module)APP.Module.StoreModules.GetModule(value.moduleId);

            if (module == null)
                throw new Exception("No existe modulo");

            ConnectionStatus connectionStatus = module.ScaleManager.IsOpen();

            if (!connectionStatus.Equals(ConnectionStatus.ACTIVO))
                connectionStatus = module.ScaleManager.OpenPort();

            if (!connectionStatus.Equals(ConnectionStatus.ACTIVO))
            {
                string message = new ActivityLogController().List(1, value.moduleId).First().message;
                throw new Exception(message);
            }

            ConfigController configCtrl = new ConfigController();
            string command = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_REQUEST_COMMAND, module.id);

            string result;
            /// Si no tiene comando configurado recupera ultimo mensaje
            if (String.IsNullOrEmpty(command))
                result = module.ScaleManager.GetMessage();
            else
                result = module.ScaleManager.Send(command).Wait(1000).GetMessage();

            scale.value = result;
            scale.dateTime = DateTime.Now;
            return Ok(scale);

        }
        public class ScaleClass
        {
            public string value;
            public DateTime dateTime;
        }


        public class ScaleModel
        {
            //TODO: debe ser obligatorio en la API
            [JsonProperty(Required = Required.Always)]
            public Guid moduleId;
        }
    }
}
