using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RepeaterModule.API;
using RepeaterModule.API.SOAP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml;

namespace TEST {
    [TestClass]
    public class RepeaterTest {
        private const string DEFAULT_ACE_OLEDB = "Microsoft.ACE.OLEDB.12.0";
        
        private string[] urls = { "http://localhost:9520", "https://localhost:9521" };


        [TestMethod]
        [TestCategory("Repeater")]
        public void PostTypeSOAP() {
            using (HttpClient client = new HttpClient()) {
                RepeaterSoapModel repeaterModel = new RepeaterSoapModel() {
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

                using (HttpResponseMessage response = client.PostAsync(@"http://localhost", content).Result) {
                    string responseContent = response.Content.ReadAsStringAsync().Result;

                    Assert.AreEqual((int)HttpStatusCode.OK, (int)response.StatusCode);
                    Assert.AreEqual("", responseContent);
                }
            }
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
    }
 }
