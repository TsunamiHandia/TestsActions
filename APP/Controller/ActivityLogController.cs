using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;
using System.Collections.ObjectModel;
using static APP.Controller.AppModulesController;

namespace APP.Controller
{
    public class ActivityLogController : Controller
    {
        [Table(Name = "ActivityLog")]
        public class ActivityLog
        {

            [Column(Name = "Id", IsPrimaryKey = true, CanBeNull = false)]
            public int? id { get; set; }

            [Column(Name = "Status")]
            public Status? status { get; set; }

            [Column(Name = "ModuleId", DataType =DataType.Text)]
            public Guid? moduleId { get; set; }

            [Column(Name = "Message", CanBeNull = false)]
            public string message { get; set; }

            [Column(Name = "StackTrace", DbType = "ntext")]
            public string stackTrace { get; set; }

            [Column(Name = "AddedAt")]
            public DateTime? addedAt { get; set; }

            public enum Status { ERROR = 0, OK = 1, INFO = 2 }

        }

        public ActivityLogController()
        {

        }
        public ActivityLogController(SQL sql) : base(sql)
        {

        }

        /// <summary>
        /// Recuperar registro por id
        /// </summary>
        /// <param name="id">Id de registro actividad</param>
        /// <returns>Registro ActivityLog</returns>
        public ActivityLog Get(int id)
        {
            DataContext context = this.getDataContext();
            ITable<ActivityLog> activityLog = context.GetTable<ActivityLog>();

            var query =
                from a in activityLog
                where a.id == id
                select a;

            if (query.Count() > 0)
            {
                return query.Single();
            }
            return null;
        }

        /// <summary>
        /// Lista registros ordenando por fecha creacion
        /// </summary>
        /// <param name="limit">Cantidad de registros filtrados</param>
        /// <returns>Listado de ActivityLog</returns>
        public List<ActivityLog> List(int limit, Guid? moduleId = null)
        {
            DataContext context = this.getDataContext();
            ITable<ActivityLog> activityLog = context.GetTable<ActivityLog>();

            IOrderedQueryable<ActivityLog> query;

            if (moduleId == null)
            {
                query =
                    from a in activityLog
                    orderby a.id descending
                    select a;
            }
            else
            {
                query =
                    from a in activityLog
                    where a.moduleId == moduleId
                    orderby a.id descending
                    select a;
            }

            return query.Take(limit).ToList();
        }

        /// <summary>
        /// Crea un nuevo registro de ActivityLog
        /// </summary>
        /// <param name="activity">Registro ActivityLog</param>
        private int Insert(ActivityLog activity)
        {
            DataContext context = this.getDataContext();
            activity.addedAt = DateTime.Now;
            return context.InsertWithInt32Identity(activity);            
        }

        /// <summary>
        /// Modifica registro existente de ActivityLog
        /// </summary>
        /// <param name="activity">Registro ActivityLog</param>
        private void Update(ActivityLog activity)
        {
            DataContext context = this.getDataContext();
            ITable<ActivityLog> activityLog = context.GetTable<ActivityLog>();

            var query =
                from a in activityLog
                where a.id == activity.id
                select a;

            ActivityLog activityDb = query.Single();
            activityDb.message = activity.message;
            context.Update(activityDb);
        }

        /// <summary>
        /// Elimina registro existente buscando por id
        /// </summary>
        /// <param name="id">Id de registro actividad</param>
        private void Delete(int id)
        {
            DataContext context = this.getDataContext();
            ITable<ActivityLog> activityLog = context.GetTable<ActivityLog>();

            var query =
                from a in activityLog
                where a.id == id
                select a;

            context.Delete(activityLog);
        }

        /// <summary>
        /// Registro registro de actividad
        /// </summary>
        /// <param name="moduleId">Identificador del modulo</param>
        /// <param name="status">Estado del registro</param>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="stackTrace">Pila de llamadas</param>
        /// <returns>Registro ActivityLog</returns>
        public ActivityLog Post(Guid moduleId, ActivityLog.Status status, string message, string stackTrace = null)
        {
            ActivityLog activityLog = new ActivityLog()
            {
                message = message.Replace("\n", "").Replace("\r", ""),
                moduleId = moduleId,
                status = status,
                stackTrace = stackTrace
            };

            if (moduleId != Guid.Empty && !IsModuleRegisterLogActive(moduleId))
            {
                return null;
            }

            activityLog.id = this.Insert(activityLog);            
            return activityLog;
        }

        /// <summary>
        /// Comprueba si registro log del modulo está activado
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <returns></returns>
        private bool IsModuleRegisterLogActive(Guid moduleId)
        {            
            AppModules module = new AppModulesController().Get(moduleId);

            if (module == null)
            {
                return false;
            }

            return (bool) module.registerLog;
        }
    }
}
