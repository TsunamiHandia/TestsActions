using APP.Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Net;
using System.Runtime.Versioning;
using static RepeaterModule.Enums;

namespace RepeaterModule.API.DB
{
    /// <summary>
    /// Para conectarnos se debe tener en el equipo el Access Database Engine:
    /// (2010) ACE.OLEDB.12.0 x64
    /// (2013) ACE.OLEDB.15.0 x64
    /// </summary>
    public class TypeAccess : TypeDB
    {
        public TypeAccess(RepeaterSqlModel model)
        {
            this.model = model;
        }

        [SupportedOSPlatform("windows")]
        public override RepeaterResponse DoAction()
        {
            RepeaterResponse repeaterResponse = new RepeaterResponse();

            try
            {
                using (OleDbConnection oleDbConnection = getAccessConnection(model, new ConfigController()))
                {
                    using (OleDbCommand oleDbCommand = new OleDbCommand(model.query, oleDbConnection))
                    {
                        oleDbCommand.CommandType = System.Data.CommandType.Text;

                        oleDbConnection.Open();

                        OleDbDataReader reader = oleDbCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

                        DataTable dataTable = new DataTable();

                        dataTable.Load(reader);

                        string jsonString = string.Empty;

                        jsonString = JsonConvert.SerializeObject(dataTable);

                        repeaterResponse.status = (int)HttpStatusCode.OK;
                        repeaterResponse.headers = new List<RepeaterHeader>();
                        repeaterResponse.headers.Add(new RepeaterHeader() { key = "records_affected", value = reader.RecordsAffected.ToString() });
                        repeaterResponse.body = jsonString;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Se ha producido un error durante la ejecución de la petición. {JsonConvert.SerializeObject(model)}", ex);
            }

            return repeaterResponse;
        }

        [SupportedOSPlatform("windows")]
        public OleDbConnection getAccessConnection(RepeaterSqlModel model, IConfigController configController = null)
        {
            string connectionString = "Provider={0};Data Source={1};Persist Security Info=False;";
            string connectionStringPassword = "Provider={0};Data Source={1};Jet OLEDB:Database Password={2};";

            string aceOledbCurVer = configController.GetValue(RepeaterConfigId.REPEATER_MODULE_ACE_OLEDB_VERSION, model.moduleId);

            string cadenaConexion = null;

            Enums.AuthenticationType? authenticationType = null;

            if (Enums.GetAuthenticationType(model.authType, out authenticationType))
                switch (authenticationType)
                {
                    case Enums.AuthenticationType.Basic:
                        cadenaConexion = string.Format(connectionStringPassword, aceOledbCurVer, model.database, model.password);
                        break;
                    default:
                        cadenaConexion = string.Format(connectionString, aceOledbCurVer, model.database);
                        break;
                }

            return new OleDbConnection(cadenaConexion);
        }
    }
}
