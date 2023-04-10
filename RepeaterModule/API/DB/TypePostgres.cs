using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using Npgsql;
using LinqToDB.Data;
using LinqToDB;

namespace RepeaterModule.API.DB
{
    public class TypePostgres : TypeDB
    {
        public TypePostgres(RepeaterSqlModel model)
        {
            this.model = model;
        }

        public override RepeaterResponse DoAction()
        {
            RepeaterResponse repeaterResponse = new RepeaterResponse();

            try 
            {
                using (NpgsqlConnection sqlConnection = getSqlConnection(model))
                {
                    using (NpgsqlCommand sqlCommand = new NpgsqlCommand(model.query, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.Text;

                        sqlConnection.Open();

                        NpgsqlDataReader reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);

                        DataTable dataTable = new DataTable();

                        dataTable.Load(reader);

                        string jsonString = string.Empty;

                        jsonString = JsonConvert.SerializeObject(dataTable);

                        repeaterResponse.status = (int)HttpStatusCode.OK;
                        repeaterResponse.headers = new List<RepeaterHeader>() { new RepeaterHeader() { key = "records_affected", value = reader.RecordsAffected.ToString() } };
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

        private NpgsqlConnection getSqlConnection(RepeaterSqlModel model)
        {
            //var db = new DataConnection(new DataOptions().UsePostgreSQL(""));
            NpgsqlConnectionStringBuilder cadenaConexion = new NpgsqlConnectionStringBuilder();            
           
            cadenaConexion.Host = model.host;

            Enums.AuthenticationType? authenticationType = null;

            if (Enums.GetAuthenticationType(model.authType, out authenticationType))
                switch (authenticationType)
                {
                    case Enums.AuthenticationType.Basic:
                        cadenaConexion.Username = model.user;
                        cadenaConexion.Password = model.password;
                        break;
                    case Enums.AuthenticationType.Ntlm:
                        cadenaConexion.IntegratedSecurity = true;
                        break;
                }

            cadenaConexion.Database = model.database;
            cadenaConexion.TrustServerCertificate = true;

            return new NpgsqlConnection (cadenaConexion.ConnectionString);
        }
    }
}
