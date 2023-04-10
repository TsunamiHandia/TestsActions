using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RepeaterModule.API.SOAP
{
    public class TypeSoap : IRepeaterType
    {
        private RepeaterSoapModel _model;
        public RepeaterSoapModel model { get => _model; }
        public TypeSoap(string body)
        {
            try
            {
                _model = JsonConvert.DeserializeObject<RepeaterSoapModel>(body);
            }
            catch
            {
                throw new Exception($"Error al deserializar el body: {body}");
            }
        }

        public RepeaterResponse DoAction()
        {
            RepeaterResponse repeaterResponse = new RepeaterResponse();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (_model.headers != null)
                        foreach (RepeaterHeader repeaterHeader in _model.headers)
                            client.DefaultRequestHeaders.Add(repeaterHeader.key, repeaterHeader.value);

                    StringContent content = new StringContent(_model.body, Encoding.UTF8, "text/xml");

                    using (HttpResponseMessage response = client.PostAsync(_model.targetUri, content).Result)
                    {
                        string soapResponse = response.Content.ReadAsStringAsync().Result;

                        repeaterResponse.status = (int)response.StatusCode;
                        repeaterResponse.headers = getResponseHeaders(response.Headers);
                        repeaterResponse.body = soapResponse;
                    }
                }
            }
            catch (Exception ex)
            {                
                throw new Exception($"Se ha producido un error durante la ejecución de la petición. {JsonConvert.SerializeObject(_model)}", ex);
            }

            return repeaterResponse;
        }

        private List<RepeaterHeader> getResponseHeaders(HttpResponseHeaders headers)
        {
            List<RepeaterHeader> returnValue = null;

            if (headers != null)
            {
                returnValue = new List<RepeaterHeader>();

                foreach (var header in headers)
                    returnValue.Add(new RepeaterHeader() { key = header.Key, value = String.Join(", ", header.Value.Select(x => x)) });
            }

            return returnValue;
        }
    }
}
