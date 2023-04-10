using System;

namespace RepeaterModule
{
    public class Enums
    {
        public enum RepeaterConfigId {
            /// <summary>
            /// Config de REPEATER
            /// </summary>        
            REPEATER_MODULE_ACE_OLEDB_VERSION = 3000
        }

        /// <summary>
        /// Devuelve un string con los valores separados por el separador (defecto = ",").
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separador"></param>
        /// <returns></returns>
        public static string GetTypeString<T>(string separador = ",")
        {
            return String.Join(separador, Enum.GetNames(typeof(T)));
        }
        public enum AuthenticationType
        {
            NoAuth = 1,
            Basic = 2,
            Ntlm = 3,
            Token = 4
        }
        /// <summary>
        /// Obtiene un AuthenticationType a partir del texto
        /// </summary>
        /// <param name="typeText"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        public static bool GetAuthenticationType(string typeText, out AuthenticationType? authenticationType)
        {
            bool returnValue = false;
            authenticationType = null;

            foreach (AuthenticationType authentication in Enum.GetValues(typeof(AuthenticationType)))
            {
                if (authentication.ToString().ToLower().Equals(typeText.ToLower()))
                {
                    authenticationType = authentication;
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }

        public enum HttpMethod
        {
            Get = 1,
            Post = 2
        }

        public enum CommunicationType
        {
            Soap = 1,
            Rest = 2,
            BBDD = 3
        }

        /// <summary>
        /// Obtiene un ComunicationType a partir del texto
        /// </summary>
        /// <param name="typeText"></param>
        /// <param name="communicationType"></param>
        /// <returns></returns>
        public static bool GetCommunicationType(string typeText, out CommunicationType? communicationType)
        {
            bool returnValue = false;
            communicationType = null;

            if (!string.IsNullOrEmpty(typeText))
                foreach (CommunicationType communication in Enum.GetValues(typeof(CommunicationType)))
                {
                    if (communication.ToString().ToLower().Equals(typeText.ToLower()))
                    {
                        communicationType = communication;
                        returnValue = true;
                        break;
                    }
                }

            return returnValue;
        }

        public enum DatabaseType
        {
            SqlServer = 1,
            SqlServerCE = 2,
            Access = 3,
            Postgres = 4
        }

        /// <summary>
        /// Obtiene un DatabaseType a partir del texto
        /// </summary>
        /// <param name="typeText"></param>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        public static bool GetDatabaseType(string typeText, out DatabaseType? databaseType)
        {
            bool returnValue = false;
            databaseType = null;

            foreach (DatabaseType database in Enum.GetValues(typeof(DatabaseType)))
            {
                if (database.ToString().ToLower().Equals(typeText.ToLower()))
                {
                    databaseType = database;
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }

    }
}
