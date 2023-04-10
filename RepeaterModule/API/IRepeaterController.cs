using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepeaterModule.API {
    public interface IRepeaterController {
        [HttpPost]
        public IActionResult Post([FromBody] object repeaterBody);
    }
}
