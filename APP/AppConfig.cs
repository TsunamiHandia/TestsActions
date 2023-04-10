using Microsoft.Extensions.Configuration;
using System.Configuration;
using System;
using System.IO;

namespace APP {
    public static class AppConfig {

        private static String? connectionString;

        /// <summary>
        /// Instancia appsettings
        /// </summary>
        /// <returns>Configurador con appsettings</returns>
        private static IConfiguration BuildConfiguration() {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            return configuration.Build();
        }

        /// <summary>
        /// Recupera ruta de database.db
        /// </summary>
        /// <param name="isOppenedInMemory">En los TEST, durante la apertura, necesitamos que sea true,
        /// para resetear el valor de connectionString que hemos hecho que sea atributo de clase para no tener que crearlo en cada llamada</param>
        /// <returns></returns>
        public static string GetConnectionString(bool isOppenedInMemory = false) {
            if (isOppenedInMemory || connectionString == null) {
                connectionString = BuildConfiguration().
                GetSection("ConnectionStrings").
                GetSection("databasePath").Value;
            }

            return connectionString;
        }

        /// <summary>
        /// Actualizacion fisica del fichero JSON de appsettings
        /// </summary>
        /// <param name="key">Indica en JSON</param>
        /// <param name="value">Nuevo valor en JSON</param>
        public static bool SetConfigurationValue(string key, string value) {
            return AddOrUpdateAppSetting(key, value);
        }

        /// <summary>
        /// Agrega o actualiza fichero JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static bool AddOrUpdateAppSetting<T>(string key, T value) {
            try {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                var sectionPath = key.Split(":")[0];

                if (!string.IsNullOrEmpty(sectionPath)) {
                    var keyPath = key.Split(":")[1];
                    jsonObj[sectionPath][keyPath] = value;
                } else {
                    jsonObj[sectionPath] = value; // if no sectionpath just set the value
                }

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);
                return true;
            } catch (ConfigurationErrorsException) {
                Console.WriteLine("Error writing app settings");
                return false;
            }
        }
    }
}
