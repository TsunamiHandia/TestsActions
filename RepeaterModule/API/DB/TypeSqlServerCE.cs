//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Net;
//using System.Data.SqlServerCe;

//namespace RepeaterModule.API.DB
//{
//    public class TypeSqlServerCE : TypeDB
//    {
//        public TypeSqlServerCE(RepeaterSqlModel model)
//        {
//            this.model = model;
//        }

//        public override RepeaterResponse DoAction()
//        {
//            RepeaterResponse repeaterResponse = new RepeaterResponse();

//            try
//            {
//                using (SqlCeConnection sqlCeConnection = getSqlCeConnection(model))
//                {
//                    using (SqlCeCommand sqlCeCommand = new SqlCeCommand(model.query, sqlCeConnection))
//                    {
//                        sqlCeCommand.CommandType = CommandType.Text;

//                        sqlCeConnection.Open();

//                        SqlCeDataReader reader = sqlCeCommand.ExecuteReader(CommandBehavior.CloseConnection);

//                        DataTable dataTable = new DataTable();

//                        dataTable.Load(reader);

//                        string jsonString = string.Empty;

//                        jsonString = JsonConvert.SerializeObject(dataTable);

//                        repeaterResponse.status = (int)HttpStatusCode.OK;
//                        repeaterResponse.headers = new List<RepeaterHeader>();
//                        repeaterResponse.headers.Add(new RepeaterHeader() { key = "records_affected", value = reader.RecordsAffected.ToString() });
//                        repeaterResponse.body = jsonString;
//                    }
//                }
//            }
//            catch
//            {
//                throw new Exception("Se ha producido un error durante la consulta.");
//            }

//            return repeaterResponse;
//        }

//        private SqlCeConnection getSqlCeConnection(RepeaterSqlModel model)
//        {
//            SqlCeConnectionStringBuilder cadenaConexion = new SqlCeConnectionStringBuilder();

//            cadenaConexion.DataSource = model.database;

//            Enums.AuthenticationType? authenticationType = null;

//            if (Enums.GetAuthenticationType(model.authType, out authenticationType))
//                switch (authenticationType)
//                {
//                    case Enums.AuthenticationType.Basic:
//                        cadenaConexion.Password = model.password;
//                        break;
//                }

//            cadenaConexion.Encrypt = false;

//            return new SqlCeConnection(cadenaConexion.ConnectionString);
//        }
//    }
//}
