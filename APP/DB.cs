using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using APP.Controller;
using Microsoft.Data.Sqlite;
using static APP.Controller.ApiActivityLogController;

namespace APP
{
    public class DB
    {
        /// <summary>
        /// Inicializa base de datos con tablas y datos predefinidos
        /// </summary>
        /// <param name="sql">Conexion SQL</param>
        /// <param name="recreate">Recrea la base de datos</param>
        /// <param name="seed">Genera datos de prueba</param>
        /// <returns></returns>
        public static SQL init(SQL sql, bool recreate = false, bool seed = false)
        {
            createTableConfig(sql, recreate, seed);
            createTableAppModules(sql, recreate, seed);
            createTableActivityLog(sql, recreate, seed);
            createTableApiActivityLog(sql, recreate, seed);            
            return sql;
        }


        /// <summary>
        /// Crea tabla de registro actividad
        /// </summary>
        /// <param name="sql">Conexion SQL</param>
        /// <param name="recreate">Recrea la base de datos</param>
        /// <param name="seed">Genera datos de prueba</param>
        /// <returns></returns>
        private static SQL createTableActivityLog(SQL sql, bool recreate, bool seed)
        {
            SqliteCommand command = sql.getSqlLiteConnection().CreateCommand();
            if (recreate)
            {                
                command.CommandText = "DROP TABLE  IF EXISTS ActivityLog";
                command.ExecuteNonQuery();
                
                command.CommandText = @"CREATE TABLE ActivityLog(
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        Status INT NOT NULL, 
                                        ModuleId uniqueidentifier NOT NULL, 
                                        Message nvarchar(1024) NOT NULL, 
                                        StackTrace ntext, 
                                        AddedAt DateTime)";
                command.ExecuteNonQuery();
            }
           

            if (seed)
            {
                Controller.ActivityLogController activityLogCtrl = new Controller.ActivityLogController(sql);
                Controller.ActivityLogController.ActivityLog activityLog = new Controller.ActivityLogController.ActivityLog();

                int rowNo = 0;
                while (rowNo <= 100)
                {
                    rowNo += 1;                    
                    activityLogCtrl.Post(Guid.Empty, Controller.ActivityLogController.ActivityLog.Status.OK, 
                        $"Seed {rowNo}");
                }
                rowNo = 0;
                while (rowNo <= 100)
                {
                    rowNo += 1;
                    activityLogCtrl.Post(Guid.Parse("91D009A2-DC3E-4266-B33D-D490A6F398F1"), Controller.ActivityLogController.ActivityLog.Status.OK,$"Seed {rowNo}");
                }
            }

            return sql;
        }
        /// <summary>
        /// Crea tabla de registro actividad Api
        /// </summary>
        /// <param name="sql">Conexion SQL</param>
        /// <param name="recreate">Recrea la base de datos</param>
        /// <param name="seed">Genera datos de prueba</param>
        /// <returns></returns>
        private static SQL createTableApiActivityLog(SQL sql, bool recreate, bool seed)
        {
            SqliteCommand command = sql.getSqlLiteConnection().CreateCommand();
            if (recreate)
            {
                command.CommandText = "DROP TABLE IF EXISTS ApiActivityLog";
                command.ExecuteNonQuery();
                command.CommandText = @"CREATE TABLE ApiActivityLog(
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                        Target INT NOT NULL, 
                                        Status INT NOT NULL, 
                                        Type INT NOT NULL, 
                                        Resource nvarchar(1024) NOT NULL,
                                        ReqHeaders ntext, 
                                        ReqBody ntext, 
                                        RespHeaders ntext, 
                                        RespBody ntext, 
                                        AddedAt DateTime)";
                command.ExecuteNonQuery();
            }
           

            if (seed)
            {
                Controller.ApiActivityLogController activityLogCtrl = new Controller.ApiActivityLogController(sql);
                Controller.ApiActivityLogController.ApiActivityLog activityLog = new Controller.ApiActivityLogController.ApiActivityLog();

                int status = 200;
                string reqBody = "{test:'body'}";
                string reqHeaders = "Autorization: Bearer j7wSac62aiwAK";
                string respBody = "{ok}";
                string respHeaders = "Accepted: json";
                ApiActivityLogController.ApiActivityLog.Target target = ApiActivityLogController.ApiActivityLog.Target.BC;
                ApiActivityLogController.ApiActivityLog.RequestType requestType = ApiActivityLogController.ApiActivityLog.RequestType.POST;
                string resource = "/api/app";

                int rowNo = 0;
                while (rowNo <= 100)
                {
                    rowNo += 1;                    
                    activityLogCtrl.Post(target, requestType, resource, reqHeaders, respBody, reqBody, respHeaders, status);
                }
            }

            return sql;
        }

        /// <summary>
        /// Crea tabla de configuracion
        /// </summary>
        /// <param name="sql">Conexion SQL</param>
        /// <param name="recreate">Recrea la base de datos</param>
        /// <param name="seed">Genera datos de prueba</param>
        /// <returns></returns>
        private static SQL createTableConfig(SQL sql, bool recreate, bool seed) {
            SqliteCommand command = sql.getSqlLiteConnection().CreateCommand();
            Controller.ConfigController configCtrl = new Controller.ConfigController(sql);
            if (recreate) {

                command.CommandText = "DROP TABLE  IF EXISTS Config";
                command.ExecuteNonQuery();

                command.CommandText = @"CREATE TABLE Config(
                                        Id INT NOT NULL, 
                                        Value NVARCHAR(2048) NOT NULL, 
                                        ModuleId uniqueidentifier NOT NULL,
                                        AddedAt DateTime,
                                        UpdatedAt DateTime,
                                        PRIMARY KEY (Id, ModuleId))";
                command.ExecuteNonQuery();
            }

            if (!sql.isOppenedInMemory) {
                configCtrl.Save(ConfigId.APP_ID, Guid.NewGuid().ToString());
                configCtrl.Save(ConfigId.APP_AUTH_TYPE, EnvType.CLOUD.ToString());
                configCtrl.Save(ConfigId.APP_AUTH_TYPE_HEADER, AuthType.BASIC.ToString());
                configCtrl.Save(ConfigId.APP_AUTOSTART, "1");
                configCtrl.Save(ConfigId.APP_API_KEY, Guid.NewGuid().ToString("N").ToUpper()); 
            }

            if (seed) {
                configCtrl.Save(Controller.ConfigId.APP_AUTH_TYPE, Controller.EnvType.CLOUD.ToString());
                configCtrl.Save(Controller.ConfigId.APP_AUTH_TYPE_HEADER, Controller.AuthType.BASIC.ToString());
                configCtrl.Save(Controller.ConfigId.APP_ID, Guid.NewGuid().ToString());
                configCtrl.Save(ConfigId.APP_API_KEY, "G3FVYY63DHAPNJ7BMYX4XM6P68I923K1");
                configCtrl.Save(ConfigId.APP_API_PORT, "9521");
                configCtrl.Save(ConfigId.APP_ENABLED_HTTPS, "1");
                configCtrl.Save(ConfigId.APP_CLIENT_CERTIFICATE, "9bcb846801625e3359d0c0e5af0943ed2a5a385b");

                configCtrl.Save(ConfigId.APP_AUTH_TENANTID, "d60693a1-c20d-421f-836a-fb176c6601bd");
                configCtrl.Save(ConfigId.APP_AUTH_CLIENTID, "b6ac30dd-5c97-4166-bc8a-63b52e4e5695");
                configCtrl.Save(ConfigId.APP_AUTH_CLIENTSECRET, "clU8Q~QzKWmgv2MXtvBseEFEf7TCu-cBeMVd2dj7");
                //configCtrl.Save(Controller.ConfigId.APP_AUTH_TOKEN, "msnlsnvsnvssmpsompvosm");
                //configCtrl.Save(Controller.ConfigId.APP_AUTH_TOKENDUE, DateTime.MinValue.ToString());
                configCtrl.Save(ConfigId.APP_AUTH_ENVIRONMENT, "Sandbox");
                configCtrl.Save(ConfigId.APP_AUTH_ONPREMISE_USER, "admin");
                configCtrl.Save(ConfigId.APP_AUTH_ONPREMISE_PASSWORD, "admin");
                configCtrl.Save(ConfigId.APP_AUTH_ONPREMISE_URL, "http://bctc:7048/bc");                
                configCtrl.Save(ConfigId.APP_COMPANY_ID, "fe6e639c-d36f-ec11-bb7f-6045bd8e52fe");

            }

            return sql;
        }
        // <summary>
        /// Crea tabla de modulos de la app
        /// </summary>
        /// <param name="sql">Conexion SQL</param>
        /// <param name="recreate">Recrea la base de datos</param>
        /// <param name="seed">Genera datos de prueba</param>
        /// <returns></returns>
        private static SQL createTableAppModules(SQL sql, bool recreate, bool seed)
        {
            SqliteCommand command = sql.getSqlLiteConnection().CreateCommand();
            if (recreate)
            {
               
                command.CommandText = "DROP TABLE IF EXISTS AppModules";
                command.ExecuteNonQuery();
                

                command.CommandText = @"CREATE TABLE AppModules(
                                        
                                        ModuleId uniqueidentifier NOT NULL,
                                        Type INT NOT NULL,
                                        Active bit, 
                                        RegisterLog bit,
                                        Name NVARCHAR(2048) NOT NULL,                                         
                                        AddedAt DateTime,
                                        PRIMARY KEY (ModuleId))";
                command.ExecuteNonQuery();
            }
            

            if (seed)
            {
                Controller.ConfigController configCtrl = new Controller.ConfigController(sql);

                Controller.AppModulesController moduleCtrl = new Controller.AppModulesController(sql);
                Controller.AppModulesController.AppModules recModule= new Controller.AppModulesController.AppModules();

                moduleCtrl.Post(Guid.Parse("91D009A2-DC3E-4266-B33D-D490A6F398F1"), Module.ModuleType.FILE,
                                true, true, "Ficheros");

                moduleCtrl.Post(Guid.Parse("00000000-0000-0000-0000-000000000004"), Module.ModuleType.PRINTER,
                                true, true, "Impresora Oficina");

                moduleCtrl.Post(Guid.Parse("a677f769-9cca-4a14-b009-98f35c197315"), Module.ModuleType.WEIGHING_MACHINE,
                                true, true, "Bascula almacen 1");

                moduleCtrl.Post(Guid.Parse("43cfa139-37b7-4e3a-a454-74651699e2ba"), Module.ModuleType.FTP,
                                true, true, "Recurso FTP 1");

                moduleCtrl.Post(Guid.Parse("ecfd003a-8454-402e-b05c-804a19736c0b"), Module.ModuleType.REPEATER,
                                true, true, "Repeater");
            }

            return sql;
        }        
        

        /// <summary>
        /// Ejecuta query SQL
        /// </summary>
        /// <param name="sql">Conexión SQL</param>
        /// <returns>Registros afectados</returns>
        public static int executeQuery(SQL sql, string query, List<SqliteParameter> sqliteParameters = null)
        {
            SqliteCommand command = sql.getSqlLiteConnection().CreateCommand();
            command.CommandText = query;
            if(sqliteParameters != null)
            {
                foreach (SqliteParameter param in sqliteParameters)
                {
                    command.Parameters.Add(param);
                }
            }                                                   
            return command.ExecuteNonQuery();
        }
    }
}
