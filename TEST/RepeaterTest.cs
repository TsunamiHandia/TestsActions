using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Xml;

namespace TEST {
    [TestClass]
    public class RepeaterTest
    {
        private const string DEFAULT_ACE_OLEDB = "Microsoft.ACE.OLEDB.12.0";
        private SQL sql;
        private string[] urls = { "http://localhost:9520", "https://localhost:9521" };
               
        
        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeSOAP()
        {            
            using (HttpClient client = new HttpClient())
            {
                RepeaterSoapModel repeaterModel = new RepeaterSoapModel()
                {
                    moduleId = new Guid("ecfd003a-8454-402e-b05c-804a19736c0b"),
                    targetUri = "http://www.dneonline.com/calculator.asmx",
                    httpMethod = "POST",
                    authType = "NONE",
                    headers = new List<RepeaterHeader>(),
                    body = createCalculatorXml()
                };

                repeaterModel.headers.Add(new RepeaterHeader() { key = "SOAPAction", value = "http://tempuri.org/Multiply" });

                string contentString = JsonConvert.SerializeObject(repeaterModel);

                StringContent content = new StringContent(contentString, Encoding.UTF8, "text/json");

                repeaterModel.authType = "NONE";

                contentString = JsonConvert.SerializeObject(repeaterModel);
                
                using (HttpResponseMessage response = client.PostAsync(@"http://localhost", content).Result)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;

                    Assert.AreEqual((int) HttpStatusCode.OK, (int) response.StatusCode);
                    Assert.AreEqual("", responseContent);
                }
            }
        }

        public class RepeaterSqlModel {
            [JsonProperty(Required = Required.Always)]
            /// <summary>
            /// Id del módulo
            /// </summary>
            public Guid moduleId { get; set; }

            [JsonProperty(Required = Required.AllowNull)]
            /// <summary>
            /// Host
            /// </summary>
            public string host { get; set; }

            [JsonProperty(Required = Required.AllowNull)]
            /// <summary>
            /// Port
            /// </summary>
            public string port { get; set; }

            [JsonProperty(Required = Required.Always)]
            /// <summary>
            /// Database. En el caso de Access, la ruta al fichero
            /// </summary>
            public string database { get; set; }

            [JsonProperty(Required = Required.Always)]
            /// <summary>
            /// Tipo de base de datos, DatabaseType [SqlServer, SqlServerCE, Access]
            /// </summary>
            public string databaseType { get; set; }

            [JsonProperty(Required = Required.Always)]
            /// <summary>
            /// Tipo de autenticación
            /// </summary>
            public string authType { get; set; }

            [JsonProperty(Required = Required.AllowNull)]
            /// <summary>
            /// User
            /// </summary>
            public string user { get; set; }

            [JsonProperty(Required = Required.AllowNull)]
            /// <summary>
            /// Password
            /// </summary>
            public string password { get; set; }

            [JsonProperty(Required = Required.Always)]
            /// <summary>
            /// Query
            /// </summary>
            public string query { get; set; }
        }
    }
