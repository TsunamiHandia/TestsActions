using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using System.IO.Packaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Windows;
using APP.Controller;

namespace APP
{
    public class SQL
    {
        private string db;        
        private SqliteConnection con;
        public const string provider = "SQLite.MS";
        public static IConfiguration Configuration { get; private set; }
        public bool isOppenedInMemory { get; private set; }

        [SupportedOSPlatform("windows7.0")]
        /// <summary>
        /// Abre conexión con SqlCe
        /// </summary>
        /// <returns></returns>
        public SQL open()
        {   
            if (!IsDbCreated())
                setDbPath(createFolderForDataBase()); 
            db = getDbPath();                      
            con = new SqliteConnection(getConnectionString(db).ConnectionString);
            con.Open();
            return this;
        }

        /// <summary>
        /// Abre conexión, en BD temporal, con SqlCe
        /// </summary>
        /// <returns></returns>
        public SQL openInMemory()
        {            
            string tmpSdf = String.Format("{0}.db", Guid.NewGuid().ToString("N"));
            string tmpPath = String.Format("{0}{1}", Path.GetTempPath(), tmpSdf);
            setDbPath(tmpPath);         
            isOppenedInMemory = true;
            getDbPath(isOppenedInMemory);

            ConfigController configCtrl = new ConfigController(null);

            con = new SqliteConnection(getConnectionString(tmpPath).ConnectionString);
            con.Open();
            return this;
        }

        /// <summary>
        /// Recupera conexión abierta
        /// </summary>
        /// <returns></returns>
        public SqliteConnection getSqlLiteConnection()
        {
            return con;
        }

        /// <summary>
        /// Construye ruta hasta fichero .db
        /// </summary>
        /// <param name="isOppenedInMemory">En los TEST, durante la apertura, necesitamos que sea true,
        /// para resetear el valor de connectionString que hemos hecho que sea atributo de clase para no tener que crearlo en cada llamada</param>
        /// <returns></returns>
        public static string getDbPath(bool isOppenedInMemory = false)
        {
            return AppConfig.GetConnectionString(isOppenedInMemory);
        }

        /// <summary>
        /// Ruta por defecto hasta fichero .db
        /// </summary>
        /// <returns></returns>
        public static void setDbPath(string path = "database.db")
        {
            AppConfig.SetConfigurationValue("ConnectionStrings:databasePath", path);
        }

        /// <summary>
        /// Construye cadena de conexion
        /// </summary>
        /// <param name="source">Origen de datos</param>
        /// <returns></returns>
        public SqliteConnectionStringBuilder getConnectionString(string source)
        {
            SqliteConnectionStringBuilder con = new SqliteConnectionStringBuilder();
            con.Add("Data Source", source);            
            return con;
        }

        /// <summary>
        /// Indica si la .db esta creada
        /// </summary>
        /// <returns></returns>
        public static bool IsDbCreated()
        {            
            return File.Exists(getDbPath());
        }
        /// <summary>
        /// crea el directorio donde se almacena la base de datos
        /// </summary>
        private static string createFolderForDataBase()
        {
            string baseDir = Environment.GetEnvironmentVariable("LocalAppData") + "\\WINTC\\";
            Directory.CreateDirectory(baseDir);
            return baseDir + "database.db";
        }
    }
}
