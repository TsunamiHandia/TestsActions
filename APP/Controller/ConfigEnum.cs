using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Controller
{    
    public enum ConfigId
    {
        /// <summary>
        /// Config de APP
        /// </summary>
        APP_AUTH_TYPE = 1,
        //APP_AUTH_DOMAIN = 2,
        APP_AUTH_ONPREMISE_USER = 3,
        APP_AUTH_ONPREMISE_PASSWORD = 4,
        APP_AUTH_TENANTID = 5,
        APP_AUTH_CLIENTID = 6,
        APP_AUTH_CLIENTSECRET = 7,
        APP_AUTH_ENVIRONMENT = 8,
        APP_AUTH_TOKEN = 9,
        APP_AUTH_TOKENDUE = 10,
        APP_AUTH_ONPREMISE_URL = 11,
        APP_ID = 12,
        APP_COMPANY_ID = 13,
        APP_AUTOSTART = 14,
        APP_AUTH_TYPE_HEADER = 15,

        /// <summary>
        /// Config de API
        /// </summary>
        APP_API_PORT = 100,
        APP_API_KEY = 200,
        APP_ENABLED_HTTPS = 300,
        APP_CLIENT_CERTIFICATE = 400,
    }
    public enum EnvType
    {
        CLOUD = 1,
        ONPREMISE = 2
    }

    public enum AuthType
    {
        BASIC = 1,
        NTLM = 2
    }

}
