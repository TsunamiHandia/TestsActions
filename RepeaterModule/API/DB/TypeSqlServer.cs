using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace RepeaterModule.API.DB
{
    public class TypeSqlServer : TypeDB
    {
        public TypeSqlServer(RepeaterSqlModel model)
        {
            this.model = model;
        }

        public override RepeaterResponse DoAction()
        {
            RepeaterResponse repeaterResponse = new RepeaterResponse();

            try 
            {
                using (SqlConnection sqlConnection = getSqlConnection(model))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(model.query, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.Text;

                        sqlConnection.Open();

                        SqlDataReader reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);

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

        private SqlConnection getSqlConnection(RepeaterSqlModel model)
        {
            SqlConnectionStringBuilder cadenaConexion = new SqlConnectionStringBuilder();

            cadenaConexion.DataSource = model.host;

            Enums.AuthenticationType? authenticationType = null;

            if (Enums.GetAuthenticationType(model.authType, out authenticationType))
                switch (authenticationType)
                {
                    case Enums.AuthenticationType.Basic:
                        cadenaConexion.Authentication = SqlAuthenticationMethod.SqlPassword;
                        cadenaConexion.UserID = model.user;
                        cadenaConexion.Password = model.password;
                        break;
                    case Enums.AuthenticationType.Ntlm:
                        cadenaConexion.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
                        break;
                }

            cadenaConexion.InitialCatalog = model.database;
            cadenaConexion.Encrypt = false;
            cadenaConexion.TrustServerCertificate = true;

            return new SqlConnection(cadenaConexion.ConnectionString);
        }
    }
}
