using Newtonsoft.Json;
using System;

namespace RepeaterModule.API.DB
{
    public class RepeaterSqlModel
    {
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
