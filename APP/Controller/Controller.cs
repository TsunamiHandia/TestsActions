using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using LinqToDB;
using LinqToDB.Mapping;

namespace APP.Controller {
    public abstract class Controller {
        private SQL sql;
        private DataContext dataContext;

        public Controller() {
            sql = new SQL();
            sql.open();            
        }

        /// <summary>
        /// Instanciar controlador con una conexión SQL ya existente
        /// </summary>
        /// <param name="sql"></param>
        public Controller(SQL sql) {
            this.sql = sql;

            if (sql == null)
                dataContext = null;
        }

        /// <summary>
        /// Recupera contexto SQL
        /// </summary>
        /// <returns>Contexto</returns>
        protected DataContext getDataContext() {
            if (dataContext == null) {
                dataContext = new DataContext(SQL.provider, sql.getConnectionString(SQL.getDbPath()).ConnectionString);
            }

            return dataContext;
        }

    }
}
