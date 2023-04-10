using API;
using APP;
using APP.Controller;
using APP.Module;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RepeaterModule;
using RepeaterModule.API;
using RepeaterModule.API.DB;
using RepeaterModule.API.SOAP;
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
using static RepeaterModule.Enums;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace TEST {
    [TestClass]
    public class RepeaterTest
    {
        private const string DEFAULT_ACE_OLEDB = "Microsoft.ACE.OLEDB.12.0";
        private SQL sql;
        private string[] urls = { "http://localhost:9520", "https://localhost:9521" };

        [TestInitialize]        
        public void Setup()
        {
            sql = new SQL();
            DB.init(sql.openInMemory(), true, true);

            //Sólo para que cargue la .dll
            RepeaterController repeaterController = new RepeaterController();

            //KestrelWebApp.Up(getBaseUrl());
            KestrelWebApp.Up(urls);
        }

        [TestCleanup]
        public void Cleanup()
        {
            KestrelWebApp.Down();
        }
        
        [TestMethod]
        [TestCategory("Repeater")]
        [SupportedOSPlatform("windows")]
        public void GetAceOledb() {
            Assert.IsNotNull(OfficeDetector.GetLastMicrosoftOledb());
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void GetAccessConnectionNoModel() {
            TypeAccess typeAccess = new TypeAccess(null);
            Assert.ThrowsException<NullReferenceException>(() => typeAccess.getAccessConnection(null));
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void GetAccessConnectionAuthBasic() {
            var mockConfigController = new Mock<IConfigController>();
            mockConfigController.Setup(m => m.GetValue(It.IsAny<Enum>(), It.IsAny<Guid>())).Returns(DEFAULT_ACE_OLEDB);

            RepeaterSqlModel repeaterSqlModel = new RepeaterSqlModel() {
                moduleId = Guid.NewGuid(),
                authType = "Basic",
                password = "xxx",
                database = "yyy"
            };

            TypeAccess typeAccess = new TypeAccess(null);
            Assert.AreEqual($"Provider={DEFAULT_ACE_OLEDB};Data Source=yyy;Jet OLEDB:Database Password=xxx;", typeAccess.getAccessConnection(repeaterSqlModel, mockConfigController.Object).ConnectionString);
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void GetAccessConnectionAuthNone() {
            var mockConfigController = new Mock<IConfigController>();
            mockConfigController.Setup(m => m.GetValue(It.IsAny<Enum>(), It.IsAny<Guid>())).Returns(DEFAULT_ACE_OLEDB);

            RepeaterSqlModel repeaterSqlModel = new RepeaterSqlModel() {
                moduleId = Guid.NewGuid(),
                authType = "NoAuth",
                password = "xxx",
                database = "yyy"
            };

            TypeAccess typeAccess = new TypeAccess(null);
            Assert.AreEqual($"Provider={DEFAULT_ACE_OLEDB};Data Source=yyy;Persist Security Info=False;", typeAccess.getAccessConnection(repeaterSqlModel, mockConfigController.Object).ConnectionString);
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void TypeSqlFactorySQL() {
            RepeaterSqlModel repeaterSqlModel = new RepeaterSqlModel() {
                moduleId = Guid.NewGuid(),
                databaseType = "SqlServer",
                authType = "Ntlm",
                password = "",
                database = "master",
                query = ""
            };

            string model = JsonConvert.SerializeObject(repeaterSqlModel);

            Assert.IsInstanceOfType<TypeSqlServer>(TypeSqlFactory.CreateTypeSql(model));
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void TypeSqlFactoryAccess() {
            RepeaterSqlModel repeaterSqlModel = new RepeaterSqlModel() {
                moduleId = Guid.NewGuid(),
                databaseType = "Access",
                authType = "NoAuth",
                password = "",
                database = "master",
                query = ""
            };

            string model = JsonConvert.SerializeObject(repeaterSqlModel);

            Assert.IsInstanceOfType<TypeAccess>(TypeSqlFactory.CreateTypeSql(model));
        }
        
        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(null)]
        [DataRow("{\"moduleId\": \"\",\"targetUri\": \"targetUri\",\"authType\": \"NONE\",\"httpMethod\": \"POST\",\"headers\": [{\"key\": \"OAPAction\",\"value\": \"tempuri.org\"}],\"body\": \"\"}")]        
        [DataRow("{\"moduleId\": \"ecfd003a-8454-402e-b05c-804a19736c0b\",\"targetUri\": \"targetUri\",\"authType\": \"NONE\",\"httpMethod\": \"POST\",\"headers\": [{}],\"body\": \"\"}")]
        public void TypeSoapRepeaterBodyIncompletOrNull(string repeaterBody) {
            Exception ex = Assert.ThrowsException<Exception>(() => new TypeSoap(repeaterBody));
            Assert.AreEqual($"Error al deserializar el body: {repeaterBody}", ex.Message);
        }
        
        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow("{   \"moduleId\": \"ecfd003a-8454-402e-b05c-804a19736c0b\",    \"targetUri\": \"http://www.dneonline.com/calculator.asmx\",    \"authType\": \"NONE\",    \"httpMethod\": \"POST\",    \"headers\": [        {            \"key\": \"SOAPAction\",            \"value\": \"http://tempuri.org/Multiply\"        }    ],    \"body\": \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><soap:Envelope xmlns:xsi=\\\"http://www.w3.org/2001/XMLSchema-instance\\\" xmlns:xsd=\\\"http://www.w3.org/2001/XMLSchema\\\" xmlns:soap=\\\"http://schemas.xmlsoap.org/soap/envelope/\\\">  <soap:Body>    <Multiply xmlns=\\\"http://tempuri.org/\\\">      <intA>688969</intA>      <intB>29</intB>    </Multiply>  </soap:Body></soap:Envelope>\"}")]
        public void TypeSoapRepeaterBody(string repeaterBody) {
            TypeSoap typeSoap = new TypeSoap(repeaterBody);

            Assert.IsInstanceOfType<TypeSoap>(typeSoap);
            Assert.AreEqual(typeSoap.model.moduleId, Guid.Parse("ecfd003a-8454-402e-b05c-804a19736c0b"));
        }

        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(null)]
        [DataRow("error")]
        public void GetCommunicationTypeError(string? typeText) {
            Enums.CommunicationType? communicationType = null;
            Assert.IsTrue(!Enums.GetCommunicationType(typeText, out communicationType));
            Assert.IsNull(communicationType);
        }

        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow("Soap")]
        [DataRow("Rest")]
        [DataRow("BBDD")]
        public void GetCommunicationType(string? typeText) {
            Enums.CommunicationType? communicationType = null;
            Assert.IsTrue(Enums.GetCommunicationType(typeText, out communicationType));
            Assert.IsNotNull(communicationType);
        }

        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(null)]
        [DataRow("type_erroneo")]
        public void RepeaterControllerPostTypeError(string? type) {

            IRepeaterController repeaterController = new RepeaterController();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.Request.Path).Returns($"/api/repeater?apikey=779B581D24034728BB256E7326B048BA&type={type}");
            mockHttpContext.Setup(m => m.Request.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>() { { "type", type } }));
            var mockControllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            ((ControllerBase) repeaterController).ControllerContext = mockControllerContext;

            Assert.AreEqual(
                new BadRequestObjectResult($"Url mal construida {{url}}/api/repeater?apikey={{apikey}}&type=[{Enums.GetTypeString<Enums.CommunicationType>()}]").Value,
                ((BadRequestObjectResult) repeaterController.Post("")).Value
                );
        }

        [TestMethod]
        [SupportedOSPlatform("windows")]
        [TestCategory("Repeater")]
        public void PostTypeAccessBadQueryMock() {
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.AssemblyResolve += MyHandler;

            string aceOledbCurVer = OfficeDetector.GetLastMicrosoftOledb();
            Assert.IsNotNull(aceOledbCurVer);

            Access access = new Access();
            access.openInMemory(aceOledbCurVer);

            Guid? moduleId = getRepeaterModuleId(sql);

            Assert.IsNotNull(moduleId);

            var mock = new Mock<IRepeaterController>();
            string peticion = JsonConvert.SerializeObject(new RepeaterSqlModel() {
                moduleId = Guid.Parse("ba0ce15e-bcf1-47c1-b1f0-abb82c841c5f"),
                host = "",
                port = "",
                database = "",
                databaseType = "Access",
                authType = "Basic",
                user = "",
                password = "",
                query = "SELECT 'tontin';"
            });
            mock.Setup(m => m.Post(It.IsAny<object>())).Returns(new OkObjectResult("OK"));

            IActionResult expected = mock.Object.Post(peticion);
            Assert.AreEqual(((ObjectResult)expected).StatusCode, StatusCodes.Status200OK);
        }
                
        [SupportedOSPlatform("windows")]
        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeAccessDefaultConnection() {
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.AssemblyResolve += MyHandler;

            string aceOledbCurVer = OfficeDetector.GetLastMicrosoftOledb();
            Assert.IsNotNull(aceOledbCurVer);

            Access access = new Access();
            access.openInMemory(aceOledbCurVer, false);

            Guid? moduleId = getRepeaterModuleId(sql);

            Assert.IsNotNull(moduleId);

            new ConfigController().Save(RepeaterConfigId.REPEATER_MODULE_ACE_OLEDB_VERSION, aceOledbCurVer, moduleId);

            RepeaterSqlModel repeaterModel = new RepeaterSqlModel() {
                moduleId = moduleId.Value,
                host = "",
                port = "",
                database = access.getOleDbConnection().DataSource,
                databaseType = "Access",
                authType = "NoAuth",
                user = "",
                password = "",
                query = "SELECT 1 AS DATO;"
            };

            string model = JsonConvert.SerializeObject(repeaterModel);

            TypeDB typeDB = TypeSqlFactory.CreateTypeSql(model);

            RepeaterResponse repeaterResponse = typeDB.DoAction();

            string respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "0"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                DATO = 1
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));
        }
               
        [SupportedOSPlatform("windows")]
        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeAccessCRUD() {
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.AssemblyResolve += MyHandler;

            Guid? moduleId = getRepeaterModuleId(sql);
            Assert.IsNotNull(moduleId);

            string aceOledbCurVer = OfficeDetector.GetLastMicrosoftOledb();
            Assert.IsNotNull(aceOledbCurVer);

            Access access = new Access();
            access.openInMemory(aceOledbCurVer);

            new ConfigController().Save(RepeaterConfigId.REPEATER_MODULE_ACE_OLEDB_VERSION, aceOledbCurVer, moduleId);

            RepeaterSqlModel repeaterModel = new RepeaterSqlModel() {
                moduleId = moduleId.Value,
                host = "",
                port = "",
                database = access.getOleDbConnection().DataSource,
                databaseType = "Access",
                authType = "Basic",
                user = "",
                password = access.pass,
                query = ""
            };

            // Creamos la tabla SqlTests
            repeaterModel.query = @"CREATE TABLE SqlTests(                                        
                                    Id AutoIncrement CONSTRAINT PrimaryKey PRIMARY KEY,
                                    Numero NUMBER,
                                    Texto TEXT)";

            string model = JsonConvert.SerializeObject(repeaterModel);

            TypeDB typeDB = TypeSqlFactory.CreateTypeSql(model);

            RepeaterResponse repeaterResponse = typeDB.DoAction();

            string respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "0"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Insertamos un registro
            repeaterModel.query = "INSERT INTO SqlTests (Id, Numero, Texto) VALUES (1, 0, 'Texto de prueba 1');";

            model = JsonConvert.SerializeObject(repeaterModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterModel.query = "SELECT * FROM SqlTests WHERE Id = 1;";

            model = JsonConvert.SerializeObject(repeaterModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "0"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                Id = 1,
                                Numero = 0.0,
                                Texto = "Texto de prueba 1"
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Actualizamos el registro que tienen Id = 1
            repeaterModel.query = "UPDATE SqlTests SET Texto = 'xxx' WHERE Id = 1;";

            model = JsonConvert.SerializeObject(repeaterModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterModel.query = "SELECT * FROM SqlTests WHERE Id = 1;";

            model = JsonConvert.SerializeObject(repeaterModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "0"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                Id = 1,
                                Numero = 0.0,
                                Texto = "xxx"
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Borramos el registro que tiene Id = 1
            repeaterModel.query = "DELETE FROM SqlTests WHERE Id = 1;";

            model = JsonConvert.SerializeObject(repeaterModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterModel.query = "SELECT * FROM SqlTests WHERE Id = 1;";

            model = JsonConvert.SerializeObject(repeaterModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "0"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeRest() {            

            IRepeaterController repeaterController = new RepeaterController();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.Request.Path).Returns($"/api/repeater?apikey=779B581D24034728BB256E7326B048BA&type=REST");
            mockHttpContext.Setup(m => m.Request.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>() { { "type", "REST" } }));
            var mockControllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            ((ControllerBase)repeaterController).ControllerContext = mockControllerContext;

            Assert.AreEqual(
                new StatusCodeResult(StatusCodes.Status501NotImplemented).StatusCode,
                ((StatusCodeResult)repeaterController.Post("")).StatusCode
                );
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeSOAPJSON() {            
            IRepeaterController repeaterController = new RepeaterController();

            // Este JSON no valida el tipo RepeaterSoapModel
            var repeaterModel = new {
                moduleId = new Guid("ecfd003a-8454-402e-b05c-804a19736c0b"),
                targetUri = "http://www.dneonline.com/calculator.asmx",
                httpMethod = "POST",
                authType = "NONE",
                body = createCalculatorXml()
            };

            string contentString = JsonConvert.SerializeObject(repeaterModel);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.Request.Path).Returns($"/api/repeater?apikey=779B581D24034728BB256E7326B048BA&type=SOAP");
            mockHttpContext.Setup(m => m.Request.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>() { { "type", "SOAP" } }));
            var mockControllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            ((ControllerBase)repeaterController).ControllerContext = mockControllerContext;

            Assert.AreEqual(
                 $"Error al deserializar el body: {contentString}",
                 ((BadRequestObjectResult)repeaterController.Post(contentString)).Value
                 );
        }

        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeSOAPBadRequest() {
            IRepeaterController repeaterController = new RepeaterController();

            RepeaterSoapModel repeaterModel = new RepeaterSoapModel() {
                moduleId = new Guid("ecfd003a-8454-402e-b05c-804a19736c0b"),
                targetUri = "url falsa",
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

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.Request.Path).Returns($"/api/repeater?apikey=779B581D24034728BB256E7326B048BA&type=SOAP");
            mockHttpContext.Setup(m => m.Request.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>() { { "type", "SOAP" } }));
            var mockControllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            ((ControllerBase)repeaterController).ControllerContext = mockControllerContext;

            Assert.IsTrue(((BadRequestObjectResult)repeaterController.Post(contentString)).Value.ToString().StartsWith("Se ha producido un error durante la ejecución de la petición."));
        }
        
        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeSOAP()
        {
            string url = getRepeaterBaseUrl() + "&type=SOAP";

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
                }
            }
        }
        
        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""5432"",""database"":""postgres"",""databaseType"":""Tipo BBDD no existente"",""authType"":""Ntlm"",""user"":"""",""password"":"""",""query"":""""}")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""1443"",""database"":""master"",""databaseType"":""Tipo BBDD no existente"",""authType"":""Ntlm"",""user"":"""",""password"":"""",""query"":""""}")]
        public void PostTypeBBDDJSON(string repeaterModel) {
            Exception ex = Assert.ThrowsException<Exception>(() => TypeSqlFactory.CreateTypeSql(repeaterModel));
            Assert.AreEqual("No se ha podido configurar el tipo de base de datos", ex.Message);
        }

        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""5432"",""database"":""postgres"",""databaseType"":""Tipo BBDD inexistente"",""authType"":""Basic"",""user"":""postgres"",""password"":""postgres"",""query"":""SELECT * FROM NOT_EXISTING_TABLE;""}")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""1443"",""database"":""master"",""databaseType"":""Tipo BBDD inexistente"",""authType"":""Basic"",""user"":""sa"",""password"":""dbatools.I0"",""query"":""SELECT * FROM NOT_EXISTING_TABLE;""}")]
        public void PostTypeBBDDErroneo(string repeaterModel) {
            Exception ex = Assert.ThrowsException<Exception>(() => TypeSqlFactory.CreateTypeSql(repeaterModel));
            Assert.AreEqual("No se ha podido configurar el tipo de base de datos", ex.Message);
        }
                
        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""5432"",""database"":""postgres"",""databaseType"":""Postgres"",""authType"":""Basic"",""user"":""postgres"",""password"":""postgres"",""query"":""select 1 as dato;""}")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""1443"",""database"":""master"",""databaseType"":""SqlServer"",""authType"":""Basic"",""user"":""sa"",""password"":""dbatools.I0"",""query"":""select 1 as dato;""}")]
        public void PostTypeAutenticacion(string repeaterModel) {
            string url = getRepeaterBaseUrl() + "&type=BBDD";

            string model = JsonConvert.SerializeObject(repeaterModel);

            TypeDB typeDB = TypeSqlFactory.CreateTypeSql(repeaterModel);

            RepeaterResponse repeaterResponse = typeDB.DoAction();

            string respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                dato = 1
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

        }
               
        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""5432"",""database"":""postgres"",""databaseType"":""Postgres"",""authType"":""Basic"",""user"":""postgres"",""password"":""postgres"",""query"":""SELECT * FROM NOT_EXISTING_TABLE;""}")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""1443"",""database"":""master"",""databaseType"":""SqlServer"",""authType"":""Basic"",""user"":""sa"",""password"":""dbatools.I0"",""query"":""SELECT * FROM NOT_EXISTING_TABLE;""}")]
        public void PostTypeBadQuery(string repeaterModel) {
            TypeDB typeDB = TypeSqlFactory.CreateTypeSql(repeaterModel);
            Exception ex = Assert.ThrowsException<Exception>(() => typeDB.DoAction());
            Assert.AreEqual($"Se ha producido un error durante la ejecución de la petición. {repeaterModel}", ex.Message);
        }

        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""1443"",""database"":""master"",""databaseType"":""SqlServer"",""authType"":""Basic"",""user"":""sa"",""password"":""dbatools.I0"",""query"":""""}")]
        //[DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""1443"",""database"":""master"",""databaseType"":""SqlServer"",""authType"":""Ntlm"",""user"":"""",""password"":"""",""query"":""""}")] // Para los test locales
        public void PostTypeSqlServerCRUD(string repeaterModel) {
            RepeaterSqlModel repeaterSqlModel = JsonConvert.DeserializeObject<RepeaterSqlModel>(repeaterModel);

            // Creamos la BBDD
            repeaterSqlModel.query = @"IF NOT EXISTS (SELECT * FROM sys.databases  WHERE name = N'TestDB') EXEC('CREATE DATABASE [TestDB]');";

            string model = JsonConvert.SerializeObject(repeaterSqlModel);

            TypeDB typeDB = TypeSqlFactory.CreateTypeSql(model);

            RepeaterResponse repeaterResponse = typeDB.DoAction();

            string respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Creamos el schema
            repeaterSqlModel.query = @"USE TestDB;
                                        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Tests') EXEC('CREATE SCHEMA [Tests]');";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Creamos la Tabla
            repeaterSqlModel.query = @"CREATE TABLE TestDB.Tests.[Destino de SQL Server] (
	                                        uniqueidentifier_field uniqueidentifier NULL,
	                                        text_field nvarchar(160) COLLATE Modern_Spanish_CI_AS NULL,
	                                        int_field int NULL,
	                                        date_field date NULL,
	                                        datetime_field datetime NULL,
	                                        numeric_field numeric(38,0) NULL,
	                                        decimal_field decimal(38,0) NULL
                                        );";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Insertamos un registro
            repeaterSqlModel.query =
                "INSERT INTO TestDB.Tests.[Destino de SQL Server] " +
                "(uniqueidentifier_field, text_field, int_field, date_field, datetime_field, numeric_field, decimal_field) " +
                "VALUES('7a8fa19b-1872-482a-aab9-9375c9e4fdd5', 'texto 1', 100, '2022-08-31', '2022-08-31 12:30:00 PM', 0, 0);";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterSqlModel.query = "SELECT * FROM TestDB.Tests.[Destino de SQL Server];";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5",
                                text_field = "texto 1",
                                int_field = 100,
                                date_field = "2022-08-31T00:00:00",
                                datetime_field = "2022-08-31T12:30:00",
                                numeric_field = 0.0,
                                decimal_field = 0.0
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Actualizamos el registro que tienen uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5"
            repeaterSqlModel.query =
                "UPDATE TestDB.Tests.[Destino de SQL Server]" +
                "SET text_field='texto actualizado'" +
                "WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5'";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterSqlModel.query = "SELECT * FROM TestDB.Tests.[Destino de SQL Server] WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5';";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5",
                                text_field = "texto actualizado",
                                int_field = 100,
                                date_field = "2022-08-31T00:00:00",
                                datetime_field = "2022-08-31T12:30:00",
                                numeric_field = 0.0,
                                decimal_field = 0.0
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Borramos el registro que tiene uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5"
            repeaterSqlModel.query = "DELETE FROM TestDB.Tests.[Destino de SQL Server] WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5';";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });                       

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterSqlModel.query = "SELECT * FROM TestDB.Tests.[Destino de SQL Server] WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5';";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));
        }

        [TestMethod]
        [TestCategory("Repeater")]
        [DataRow(@"{""moduleId"":""ecfd003a-8454-402e-b05c-804a19736c0b"",""host"":""localhost"",""port"":""5432"",""database"":""postgres"",""databaseType"":""Postgres"",""authType"":""Basic"",""user"":""postgres"",""password"":""postgres"",""query"":""""}")]        
        public void PostTypePostgresCRUD(string repeaterModel) {
            RepeaterSqlModel repeaterSqlModel = JsonConvert.DeserializeObject<RepeaterSqlModel>(repeaterModel);

            // Creamos la Tabla
            repeaterSqlModel.query = @"CREATE TABLE Destino_de_PostgreSQL (
                                        uniqueidentifier_field uuid NULL,
                                        text_field varchar(160) NULL,
                                        int_field integer NULL,
                                        date_field date NULL,
                                        datetime_field timestamp NULL,
                                        numeric_field numeric NULL,
                                        decimal_field decimal NULL
                                    );";

            string model = JsonConvert.SerializeObject(repeaterSqlModel);

            TypeDB typeDB = TypeSqlFactory.CreateTypeSql(model);

            RepeaterResponse repeaterResponse = typeDB.DoAction();

            string respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Insertamos un registro
            repeaterSqlModel.query =
                "INSERT INTO Destino_de_PostgreSQL (uniqueidentifier_field, text_field, int_field, date_field, datetime_field, numeric_field, decimal_field) " + 
                "VALUES ('7a8fa19b-1872-482a-aab9-9375c9e4fdd5', 'texto 1', 100, '2022-08-31', '2022-08-31 12:30:00', 0, 0);";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterSqlModel.query = "SELECT * FROM Destino_de_PostgreSQL;";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5",
                                text_field = "texto 1",
                                int_field = 100,
                                date_field = "2022-08-31T00:00:00",
                                datetime_field = "2022-08-31T12:30:00",
                                numeric_field = 0.0,
                                decimal_field = 0.0
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Actualizamos el registro que tienen uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5"
            repeaterSqlModel.query =
                "UPDATE Destino_de_PostgreSQL " +
                "SET text_field = 'texto actualizado' " +
                "WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5'";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterSqlModel.query = "SELECT * FROM Destino_de_PostgreSQL WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5';";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = JsonConvert.SerializeObject(
                            new[] {
                                new {
                                uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5",
                                text_field = "texto actualizado",
                                int_field = 100,
                                date_field = "2022-08-31T00:00:00",
                                datetime_field = "2022-08-31T12:30:00",
                                numeric_field = 0.0,
                                decimal_field = 0.0
                                }
                            }
                        )
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Borramos el registro que tiene uniqueidentifier_field = "7a8fa19b-1872-482a-aab9-9375c9e4fdd5"
            repeaterSqlModel.query = "DELETE FROM Destino_de_PostgreSQL WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5';";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));

            // Leemos los registros que tienen Id = 1
            repeaterSqlModel.query = "SELECT * FROM Destino_de_PostgreSQL WHERE uniqueidentifier_field = '7a8fa19b-1872-482a-aab9-9375c9e4fdd5';";

            model = JsonConvert.SerializeObject(repeaterSqlModel);

            typeDB = TypeSqlFactory.CreateTypeSql(model);

            repeaterResponse = typeDB.DoAction();

            respuestaEsperada = JsonConvert.SerializeObject(new RepeaterResponse() {
                status = 200,
                headers = new List<RepeaterHeader>() {
                            new RepeaterHeader() {
                                key = "records_affected",
                                value = "-1"
                            }
                        },
                body = "[]"
            });

            Assert.AreEqual(respuestaEsperada, JsonConvert.SerializeObject(repeaterResponse));
        }

        /// <summary>
        /// Registrar module en la APP
        /// </summary>
        /// <param name="moduleId">Guid del modulo a registrar</param>
        public void RegisterModule(Guid moduleId)
        {
            ModuleEvent moduleEvent = new ModuleEvent();
            moduleEvent.module = new RepeaterModule.Module();
            moduleEvent.module.id = moduleId;
            moduleEvent.module.name = String.Empty;
            moduleEvent.module.type = ModuleType.REPEATER;
            moduleEvent.module.active = true;
            moduleEvent.module.addedAt = DateTime.UtcNow;
            moduleEvent.module.up();

            StoreModules.modulesCollection.Add(moduleId.ToString(), moduleEvent);
        }

        private string createCalculatorXml() {
            XmlDocument xmlDocument = new XmlDocument();
            XmlNode xmlNode = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDocument.AppendChild(xmlNode);

            XmlElement soapEnvelope = xmlDocument.CreateElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
            soapEnvelope.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            soapEnvelope.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
            soapEnvelope.SetAttribute("xmlns:soap", "http://schemas.xmlsoap.org/soap/envelope/");
            xmlDocument.AppendChild(soapEnvelope);

            XmlNode bodyNode = xmlDocument.CreateElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
            soapEnvelope.AppendChild(bodyNode);

            XmlNode multiplyNode = xmlDocument.CreateElement("Multiply", "http://tempuri.org/");

            XmlNode intNode = xmlDocument.CreateElement("intA", "http://tempuri.org/");
            intNode.InnerText = "688969";
            multiplyNode.AppendChild(intNode);

            intNode = xmlDocument.CreateElement("intB", "http://tempuri.org/");
            intNode.InnerText = "29";
            multiplyNode.AppendChild(intNode);

            bodyNode.AppendChild(multiplyNode);


            return xmlDocument.OuterXml;
        }

        /// <summary>
        /// Obtiene el ID del módulo Repeater
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Guid? getRepeaterModuleId(SQL sql) {
            return new AppModulesController(sql).List(1, ModuleType.REPEATER).FirstOrDefault()?.moduleId;
        }

        private Assembly MyHandler(object sender, ResolveEventArgs args) {
            return Assembly.LoadFile(args.RequestingAssembly.Location);
        }

        private string getBaseUrl() {
            ConfigController configCtrl = new ConfigController();

            string port = configCtrl.GetValue(ConfigId.APP_API_PORT);

            if (string.IsNullOrEmpty(port))
                port = "80";

            return $"http://localhost:{port}/";
        }

        private string getRepeaterBaseUrl() {
            ConfigController configCtrl = new ConfigController();
            string apiKey = configCtrl.GetValue(ConfigId.APP_API_KEY);

            return $"{getBaseUrl()}api/repeater?apikey={apiKey}";
        }
    }

   

    public class ModuleTest    {

        public Guid moduleId;
    }
}
