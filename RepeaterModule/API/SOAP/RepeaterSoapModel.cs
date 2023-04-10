using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RepeaterModule.API.SOAP
{
    public class RepeaterSoapModel
    {
        [JsonProperty(Required = Required.Always)]
        /// <summary>
        /// Id del módulo
        /// </summary>
        public Guid moduleId { get; set; }

        [JsonProperty(Required = Required.Always)]
        /// <summary>
        /// URL destino
        /// </summary>
        public string targetUri { get; set; }

        [JsonProperty(Required = Required.Always)]
        /// <summary>
        /// Enumerador con tipo de autentificación: None, Basic, NTLM, Token
        /// </summary>
        public string authType { get; set; }

        [JsonProperty(Required = Required.Always)]
        /// <summary>
        /// Enumerador con el tipo de método HTTP: GET, POST
        /// </summary>
        public string httpMethod { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        /// <summary>
        /// Cabeceras de mensaje a emitir
        /// </summary>
        public List<RepeaterHeader> headers { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        /// <summary>
        /// Cuerpo de mensaje a emitir
        /// </summary>
        public string body { get; set; }


    }
}
