using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Mapping;
using APP.Module;

namespace APP.Controller
{
    public class AppModulesController : Controller
    {
        [Table(Name = "AppModules")]
        public class AppModules
        {
            [Column(Name = "ModuleId", IsPrimaryKey = true, CanBeNull = false, DataType = DataType.Text)]
            public Guid? moduleId { get; set; }

            [Column(Name = "Type")]
            public ModuleType? type { get; set; }

            [Column(Name = "Active")]
            public bool? active { get; set; }

            [Column(Name = "RegisterLog")]
            public bool? registerLog { get; set; }

            [Column(Name = "Name")]
            public string name { get; set; }

            [Column(Name = "AddedAt")]
            public DateTime? addedAt { get; set; }

        }

        public AppModulesController()
        {

        }
        public AppModulesController(SQL sql) : base(sql)
        {

        }

        /// <summary>
        /// Recuperar registro
        /// </summary>
        /// <param name="id">ID del modulo</param>
        /// <returns>Registro AppModules</returns>
        public AppModules Get(Guid? id = null)
        {
            DataContext context = this.getDataContext();
            ITable<AppModules> activityLog = context.GetTable<AppModules>();

            var query =
                from a in activityLog
                where a.moduleId == id
                select a;

            if (query.Count() > 0)
            {
                return query.Single();
            }
            return null;
        }

        /// <summary>
        /// Lista modulos
        /// </summary>
        /// <param name="limit">Cantidad de registros filtrados</param>
        /// <returns>Listado de AppModules</returns>
        public List<AppModules> List(int limit, ModuleType? type = null)
        {
            DataContext context = this.getDataContext();
            ITable<AppModules> activityLog = context.GetTable<AppModules>();

            IOrderedQueryable<AppModules> query;

            if (type != null) {
                query =
                    from a in activityLog
                    where a.type == type
                    orderby a.moduleId descending
                    select a;
            }

            else { 
                query =
                    from a in activityLog
                    orderby a.moduleId descending
                    select a;
            }

            return query.Take(limit).ToList();
        }

        /// <summary>
        /// Crea un nuevo registro en AppModules
        /// </summary>
        /// <param name="activity">Registro AppModules</param>
        private void Insert(AppModules appModule)
        {
            DataContext context = this.getDataContext();
            ITable<AppModules> appModules = context.GetTable<AppModules>();
            appModule.addedAt = DateTime.UtcNow;            
            context.Insert(appModule);
        }

        /// <summary>
        /// Modifica registro existente de AppModules
        /// </summary>
        /// <param name="activity">Registro AppModules</param>
        private void Update(AppModules appModule)
        {
            DataContext context = this.getDataContext();
            ITable<AppModules> activityLog = context.GetTable<AppModules>();

            var query =
                from a in activityLog
                where a.moduleId == appModule.moduleId
                select a;

            AppModules activityDb = query.Single();
            activityDb.name = appModule.name;
            activityDb.type = appModule.type;
            activityDb.active = appModule.active;
            activityDb.registerLog = appModule.registerLog;
            context.Update(activityDb);
        }

        /// <summary>
        /// Elimina registro existente buscando por id
        /// </summary>
        /// <param name="id">ID del modulo</param>
        private void Delete(Guid id)
        {
            DataContext context = this.getDataContext();
            ITable<AppModules> activityLog = context.GetTable<AppModules>();

            var query =
                from a in activityLog
                where a.moduleId == id
                select a;
            
            context.Delete(query.Single());
        }

        /// <summary>
        /// Registro registro de modulos
        /// </summary>
        /// <param name="moduleId">Identificador del modulo</param>
        /// <param name="type">Tipo del modulo</param>
        /// <param name="active">Indica si esta activo</param>
        /// <param name="registerLog">Indica si debe registrar log actividad</param>
        /// <param name="name">Nombre del modulo</param>
        /// <returns>Registro ActivityLog</returns>
        public AppModules Post(Guid moduleId, ModuleType type, bool active, bool registerLog, string name = null)
        {
            AppModules activityLog = new AppModules()
            {
                moduleId = moduleId,
                type = type,
                active = active,
                registerLog = registerLog,
                name = name
            };
            this.Insert(activityLog);
            return activityLog;
        }

        /// <summary>
        /// Modifica registro existente en modulos
        /// </summary>
        /// <param name="module">AppModules</param>
        public void Save(AppModules module)
        {
            this.Update(module);
        }
    }
}
