using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepeaterModule.API
{
    public class RepeaterHeader
    {
        [JsonProperty(Required = Required.Always)]
        public string key { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string value { get; set; }
    }
}
