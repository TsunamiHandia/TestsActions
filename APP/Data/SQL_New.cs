using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.Specialized;

namespace APP
{
    public class SQL_New
    {
        private const string db = "database.db";
        //private const string db = "C:/Users/chernandez/source/repos/WIN_TC/WIN_TC/APP/database.db";
        //private string db = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),@"APP\database.db");
        public const string tabla_config = "Configuracion";
        public const string tabla_log = "Logs";
        public const string tabla_config_columnas = "'Id','TenantId','Entorno'";
        public const string tabla_log_columnas = "'Id','Nombre','Valor'";
        public const string tabla_config_def_columnas = "Id PRIMARY KEY,TenantId,Entorno";
        public const string tabla_log_def_columnas = "Id PRIMARY KEY,Nombre,Valor";
        public const string tabla_config_valores = "1,'1439111a-14f7-4bd0-a874-9563c473eb17',1";
        public const string tabla_log_valores = "1,'Testing','Value'";
        private SQLiteConnection con;        

        public SQLiteConnection Init()
        {
            //SQLiteConnection.CreateFile("C:/Users/chernandez/source/repos/WIN_TC/WIN_TC/APP/database.db");
            //con = new SQLiteConnection(@"Data Source=C:/Users/chernandez/source/repos/WIN_TC/WIN_TC/APP/database.db;Version=3;");
            con = new SQLiteConnection(String.Format("Data Source= {0}; Version = 3; ", db));
            con.Open();
            if (!ExisteTabla(tabla_config) || !ExisteTabla(tabla_log)) {
                LimpiarBBDD();
                CrearTabla();
                GenerarDatos();            
            }
            return con;
        }

        public void LimpiarBBDD()
        {           
            SQLiteCommand command = con.CreateCommand();
            command.CommandText = String.Format("DROP TABLE IF EXISTS {0}", tabla_config);
            command.ExecuteNonQuery();
            command.CommandText = String.Format("DROP TABLE IF EXISTS {0}", tabla_log);
            command.ExecuteNonQuery();
        }


        public void CrearTabla()
        {            
            SQLiteCommand command = con.CreateCommand();
            command.CommandText = String.Format("CREATE TABLE {0}({1})", tabla_config, tabla_config_def_columnas);
            command.ExecuteNonQuery();
            command.CommandText = String.Format("CREATE TABLE {0}({1})", tabla_log, tabla_log_def_columnas);
            command.ExecuteNonQuery();
        }

        public void GenerarDatos()
        {            
            SQLiteCommand command = con.CreateCommand();
            command.CommandText = String.Format("INSERT INTO {0}({1}) VALUES({2})", tabla_config, tabla_config_columnas,
                        tabla_config_valores);
            command.ExecuteNonQuery();
            command.CommandText = String.Format("INSERT INTO {0}({1}) VALUES({2})", tabla_log, tabla_log_columnas,
                        tabla_log_valores);
            command.ExecuteNonQuery();
        }

        public void Cerrar()
        {
            con.Close();
            con = null;
        }


        protected bool ExisteTabla(String tabla)
        {
            SQLiteCommand command = con.CreateCommand();
            command.CommandText = String.Format(@"SELECT COUNT(*) > 0 as contador FROM sqlite_master 
                                                     WHERE type='table' AND name='{0}';", tabla);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            bool existe = false;
            if (reader.GetInt32(reader.GetOrdinal("contador")) > 0)
            {
                existe = true;
            }
            reader.Close();
            reader.Dispose();
            return existe;
        }

        /* protected void CrearTabla(string columnas)
          {
              InicializarDB();
              SQLiteCommand command = con.CreateCommand();
              command.CommandText = String.Format("CREATE TABLE {0}({1})", tabla, columnas);
              command.ExecuteNonQuery();
              CerrarDB();
          }

        

         protected NameValueCollection RecuperarRegistroUnico()
         {
             InicializarDB();            
             SQLiteCommand command = con.CreateCommand();
             command.CommandText = String.Format(@"SELECT * FROM {0} LIMIT 1;", tabla);
             SQLiteDataReader reader = command.ExecuteReader();
             NameValueCollection valores = null;
             if (reader.Read())
             {
                 valores = reader.GetValues();
             }            
             reader.Close();
             reader.Dispose();
             CerrarDB();

             return valores;
         }

         protected void RecuperarRegistroUnicoa()
         {
             InicializarDB();
             con.


             return valores;
         }

         protected void InsertarRegistro(string columnas, string valores)
         {
             InicializarDB();
             SQLiteCommand command = con.CreateCommand();            
             command.CommandText = String.Format("INSERT INTO {0}({1}) VALUES({2})", tabla, columnas, valores);
             command.ExecuteNonQuery();
             CerrarDB();
         }


         protected void EjecutarConsulta(string consulta)
         {
             throw new NotImplementedException("No implementado");
             InicializarDB();
             //TO-DO
             CerrarDB();
         }*/


    }
}
