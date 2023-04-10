using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepeaterModule.API.DB
{
    public abstract class TypeSqlFactory
    {
        /// <summary>
        /// Genera un TypeSql, según el valor del campo databaseType Enums.DataBaseType (SqlServer, Acess,...)
        /// </summary>
        /// <param name="repeaterSqlModelString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TypeDB CreateTypeSql(string repeaterSqlModelString)
        {
            RepeaterSqlModel model;

            try
            {
                model = JsonConvert.DeserializeObject<RepeaterSqlModel>(repeaterSqlModelString);
            }
            catch
            {
                throw new Exception($"Error al deserializar el body: {repeaterSqlModelString}");
            }

            Enums.DatabaseType? databaseType = null;

            if (Enums.GetDatabaseType(model.databaseType, out databaseType))
                switch (databaseType)
                {
                    case Enums.DatabaseType.SqlServer:
                        return new TypeSqlServer(model);
                    //case Enums.DatabaseType.SqlServerCE:
                    //    return new TypeSqlServerCE(model);
                    case Enums.DatabaseType.Access:
                        return new TypeAccess(model);
                    case Enums.DatabaseType.Postgres:
                        return new TypePostgres(model);
                }

            throw new Exception($"No se ha podido configurar el tipo de base de datos");
        }
    }
}
