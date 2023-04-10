using System;
using System.Linq;
using System.Text;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using static APP.Controller.ApiActivityLogController;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace APP.Controller
{

    public class ODataV4Controller
    {
        private string tenantId;
        private string clientId;
        private string clientSecret;
        private string accessToken;
        private DateTimeOffset dueAccessToken;
        private string company;
        private string environment;
        private string urlBase;
        private string user;
        private string pass;
        private string idTerminal;
        private const string urlScope = "https://api.businesscentral.dynamics.com/.default";
        private const string urlAPI = "https://api.businesscentral.dynamics.com/v2.0/{0}/{1}{2}";
        private const string basicAut = "{0}:{1}";
        private EnvType env { get; set; }
        private AuthType auth { get; set; }

        public enum requestType { GET, POST, PUT, PATCH, DELETE }

        public ODataV4Controller(string tenantId, string clientId, string clientSecret, string company, string environment)
        {
            this.tenantId = tenantId;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.company = company;
            this.environment = environment;
            this.env = EnvType.CLOUD;
        }

        public ODataV4Controller(string urlBase, string user, string pass)
        {
            this.urlBase = urlBase;
            this.user = user;
            this.pass = pass;
            this.env = EnvType.ONPREMISE;
        }

        public ODataV4Controller()
        {
            ConfigController configCtrl = new ConfigController();
            urlBase = configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_URL);
            user = configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_USER);
            pass = configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_PASSWORD);
            env = (EnvType)Enum.Parse(typeof(EnvType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE));
            auth = (AuthType)Enum.Parse(typeof(AuthType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE_HEADER));
            tenantId = configCtrl.GetValue(ConfigId.APP_AUTH_TENANTID);
            clientId = configCtrl.GetValue(ConfigId.APP_AUTH_CLIENTID);
            clientSecret = configCtrl.GetValue(ConfigId.APP_AUTH_CLIENTSECRET);
            environment = configCtrl.GetValue(ConfigId.APP_AUTH_ENVIRONMENT);
            accessToken = configCtrl.GetValue(ConfigId.APP_AUTH_TOKEN);
            DateTimeOffset.TryParse(configCtrl.GetValue(ConfigId.APP_AUTH_TOKENDUE), out dueAccessToken);
            company = configCtrl.GetValue(ConfigId.APP_COMPANY_ID);
            idTerminal = configCtrl.GetValue(ConfigId.APP_ID);
        }

        public ODataV4Controller(SQL sql)
        {
            ConfigController configCtrl = new ConfigController(sql);
            urlBase = configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_URL);
            user = configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_USER);
            pass = configCtrl.GetValue(ConfigId.APP_AUTH_ONPREMISE_PASSWORD);
            env = (EnvType)Enum.Parse(typeof(EnvType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE));
            auth = (AuthType)Enum.Parse(typeof(AuthType), configCtrl.GetValue(ConfigId.APP_AUTH_TYPE_HEADER));
            tenantId = configCtrl.GetValue(ConfigId.APP_AUTH_TENANTID);
            clientId = configCtrl.GetValue(ConfigId.APP_AUTH_CLIENTID);
            clientSecret = configCtrl.GetValue(ConfigId.APP_AUTH_CLIENTSECRET);
            environment = configCtrl.GetValue(ConfigId.APP_AUTH_ENVIRONMENT);
            accessToken = configCtrl.GetValue(ConfigId.APP_AUTH_TOKEN);
            DateTimeOffset.TryParse(configCtrl.GetValue(ConfigId.APP_AUTH_TOKENDUE), out dueAccessToken);
        }

        /// <summary>
        /// Invoca una petición a Business Central
        /// Regenera automaticamente token de acceso
        /// </summary>
        /// <param name="type">Metodo HTTP</param>
        /// <param name="resource">Recurso o URL absoluta</param>
        /// <param name="json">Cuerpo de JSON enviado</param>
        /// <returns>Respuesta HttpResponseMessage con resultado</returns>
        public HttpResponseMessage request(requestType type, string resource, string json = null)
        {
            ApiActivityLogController apiActivityLogCtrl = new ApiActivityLogController();

            string url;
            string requestHeaders = null;
            string requestBody = null;
            string responseHeaders = null;
            string responseBody = null;
            int statusCode = 0;
            HttpClient client = new HttpClient(); ;
            var defaultRequestHeaders = client.DefaultRequestHeaders;

            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            if (env == EnvType.ONPREMISE)
            {
                switch (auth)
                {
                    case AuthType.BASIC:
                        //si es OnPrem la url es (base + endpoint) y la autenticación es Basica   
                        defaultRequestHeaders.Add("Authorization", String.Format("Basic {0}", Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format(basicAut, user, pass)))));
                        break;
                    case AuthType.NTLM:
                        //si es OnPrem la url es (base + endpoint) y la autenticación es NTLM              
                        client = new HttpClient(new HttpClientHandler { Credentials = new NetworkCredential(user, pass) });
                        break;
                    default:
                        break;
                }

                url = Uri.IsWellFormedUriString(resource, UriKind.Relative) ?
                   String.Format(urlBase + resource) : resource;
            }
            else if (env == EnvType.CLOUD)
            {
                getAccessToken(true);

                url = Uri.IsWellFormedUriString(resource, UriKind.Relative) ?
                   String.Format(urlAPI, tenantId, environment, resource) : resource;

                //si es SandBox la autenticación es Bearer
                defaultRequestHeaders.Add("Authorization", String.Format("Bearer {0}", this.accessToken));
            }
            else
            {
                throw new NotImplementedException($"No se admite tipo {env}");
            }

            HttpResponseMessage response = null;

            foreach (var header in client.DefaultRequestHeaders)
            {
                requestHeaders += (String.Format("{0}: {1}{2}{2}", header.Key, string.Join(",", header.Value), Environment.NewLine));
            }

            requestBody = String.IsNullOrEmpty(json) ? "" : json;
            ApiActivityLog.RequestType method = (ApiActivityLog.RequestType)Enum.Parse(typeof(ApiActivityLog.RequestType), type.ToString());

            switch (type)
            {
                case requestType.GET:
                    response = client.GetAsync(url).Result;
                    break;

                case requestType.POST:
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    response = client.PostAsync(url, data).Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                    break;
                case requestType.PUT:
                    throw new NotImplementedException();
                case requestType.PATCH:
                    throw new NotImplementedException();
                case requestType.DELETE:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            foreach (var header in response.Headers)
            {
                responseHeaders += (String.Format("{0}: {1}{2}{2}", header.Key, string.Join(",", header.Value), Environment.NewLine));
            }

            responseBody = response.Content.ReadAsStringAsync().Result;
            statusCode = (int)response.StatusCode;
            apiActivityLogCtrl.Post(ApiActivityLog.Target.BC, method, resource, requestHeaders, requestBody, responseHeaders, responseBody, statusCode);

            return response;
        }

        /// <summary>
        /// Solicita un nuevo token a OAuth 2.0
        /// </summary>
        /// <param name="force">Forzar la regeneración del token</param>
        private void getAccessToken(bool force = false)
        {
            if (!force && !String.IsNullOrEmpty(accessToken) && DateTime.UtcNow < dueAccessToken)
            {
                return;
            }

            try
            {
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithTenantId(tenantId)
                    .Build();

                AuthenticationResult result = app.AcquireTokenForClient(new string[] { urlScope }).ExecuteAsync().Result;
                this.accessToken = result.AccessToken;
                this.dueAccessToken = result.ExpiresOn;
                ConfigController configCtrl = new ConfigController();
                configCtrl.Save(ConfigId.APP_AUTH_TOKEN, this.accessToken);
                configCtrl.Save(ConfigId.APP_AUTH_TOKENDUE, this.dueAccessToken.ToString());
            }
            catch (Exception e)
            {
                if (e.InnerException is MsalServiceException)
                {
                    MsalServiceException msalExcep = (MsalServiceException)e.InnerException;
                    ApiActivityLogController apiActivityLogCtrl = new ApiActivityLogController();
                    apiActivityLogCtrl.Post(ApiActivityLog.Target.BC, ApiActivityLog.RequestType.POST, "login.microsoftonline.com", null, null, msalExcep.Headers.ToString(), msalExcep.ResponseBody, msalExcep.StatusCode);
                }
                throw;
            }

        }

        /// <summary>
        /// Construye parametro del recurso
        /// </summary>
        /// <param name="key">Nombre de parametro</param>
        /// <param name="value">Valor del parametro</param>
        /// <param name="firstParam">Es primer parametro de la URI</param>
        /// <returns>Parametro formateado</returns>
        public static string AddQueryParam(string key, string value, bool firstParam = false)
        {
            string queryParam;

            if (firstParam)
                queryParam = String.Format("?{0}={1}", key, Uri.EscapeDataString(value));
            else
                queryParam = String.Format("&{0}={1}", key, Uri.EscapeDataString(value));

            return queryParam;
        }
    }
}
