using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepeaterModule;
using RepeaterModule.API;
using RepeaterModule.API.DB;
using RepeaterModule.API.SOAP;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace API {
    [ApiController]
    [Route("/api/repeater")]
    public class RepeaterController : ControllerBase, IRepeaterController {
        [HttpPost]
        public IActionResult Post([FromBody] object repeaterBody) {
            var queryParams =this.ControllerContext.HttpContext.Request.Query;
            IActionResult returnValue = BadRequest();

            Enums.CommunicationType? communicationType = null;

            if (!string.IsNullOrEmpty(queryParams["type"]) && Enums.GetCommunicationType(queryParams["type"], out communicationType)) {
                switch (communicationType) {
                    case Enums.CommunicationType.Soap:
                        IRepeaterType typeSoap = null;

                        try {
                            typeSoap = new TypeSoap(repeaterBody.ToString());
                        } catch (Exception ex) {
                            returnValue = BadRequest(ex.Message);
                            break;
                        }

                        try {
                            returnValue = Ok(typeSoap.DoAction());
                        } catch (Exception ex) {
                            returnValue = BadRequest(ex.Message);
                            break;
                        }

                        break;
                    case Enums.CommunicationType.Rest:
                        returnValue = new StatusCodeResult(StatusCodes.Status501NotImplemented);
                        break;
                    case Enums.CommunicationType.BBDD:
                        TypeDB typeSql = TypeSqlFactory.CreateTypeSql(repeaterBody.ToString());
                        returnValue = Ok(typeSql.DoAction());
                        break;
                }
            } else
                returnValue = BadRequest($"Url mal construida {{url}}/api/repeater?apikey={{apikey}}&type=[{Enums.GetTypeString<Enums.CommunicationType>()}]");
            
            return returnValue;
        }
    }

    [SupportedOSPlatform("windows")]
    public class InstalledMicrosoftAceOledb {
        public List<string> installedMicrosoftAceOledb = new();

    }
}
