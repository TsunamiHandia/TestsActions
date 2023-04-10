using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace APP.Controller
{
    public class ApiActivityLogController : Controller
    {
        [Table(Name = "ApiActivityLog")]
        public class ApiActivityLog
        {            

            [Column(Name = "Id", IsPrimaryKey = true, CanBeNull = false)]            
            public long? id { get; set; }

            [Column(Name = "Target")]
            public Target? target { get; set; }

            [Column(Name = "Status")]
            public int? status { get; set; }

            [Column(Name = "Type")]
            public RequestType? requestType { get; set; }

            [Column(Name = "Resource", CanBeNull = false)]
            public string resource { get; set; }

            [Column(Name = "ReqHeaders", DbType = "ntext")]
            public string reqHeaders{ get; set; }

            [Column(Name = "ReqBody",DbType = "ntext")]
            public string reqBody { get; set; }

            [Column(Name = "RespHeaders", DbType = "ntext")]
            public string respHeaders { get; set; }

            [Column(Name = "RespBody", DbType = "ntext")]
            public string respBody { get; set; }

            [Column(Name = "AddedAt")]
            public DateTime? addedAt { get; set; }
            
            public enum Target { APP = 0, BC = 1 }
            public enum RequestType { GET = 0, POST = 1, DELETE = 2 }
        }

        public ApiActivityLogController()
        {
            
        }
        public ApiActivityLogController(SQL sql) : base(sql)
        {
            
        }

        /// <summary>
        /// Recuperar registro por id
        /// </summary>
        /// <param name="id">Id de registro actividad Api</param>
        /// <returns>Registro ActivityLog</returns>
        public ApiActivityLog Get(int id)
        {
            DataContext context = getDataContext();
            ITable<ApiActivityLog> activityLog = context.GetTable<ApiActivityLog>();

            var query =
                from a in activityLog
                where a.id == id
                select a;
            
            if(query.Count() > 0)
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
        public List<ApiActivityLog> List(int limit)
        {
            DataContext context = getDataContext();
            ITable<ApiActivityLog> activityLog = context.GetTable<ApiActivityLog>();

            var query =                
                from a in activityLog   
                orderby a.id descending
                select a;

            return query.Take(limit).ToList();                  
        }

        /// <summary>
        /// Crea un nuevo registro de ActivityLog
        /// </summary>
        /// <param name="activity">Registro ActivityLog</param>
        private int Insert(ApiActivityLog activity)
        {                        
            DataContext context = getDataContext();                                              
            ITable<ApiActivityLog> activityLog = context.GetTable<ApiActivityLog>();
            activity.addedAt = DateTime.Now;                        
            return context.InsertWithInt32Identity(activity);
        }

        /// <summary>
        /// Modifica registro existente de ActivityLog
        /// </summary>
        /// <param name="activity">Registro ActivityLog</param>
        private void Update(ApiActivityLog activity)
        {
            DataContext context = getDataContext();
            ITable<ApiActivityLog> activityLog = context.GetTable<ApiActivityLog>();

            var query =
                from a in activityLog
                where a.id == activity.id
                select a;

            ApiActivityLog activityDb = query.Single();
            activityDb.resource = activity.resource;
            context.Update(activityDb);
        }

        /// <summary>
        /// Elimina registro existente buscando por id
        /// </summary>
        /// <param name="id">Id de registro actividad</param>
        private void Delete(int id)
        {
            DataContext context = getDataContext();
            ITable<ApiActivityLog> activityLog = context.GetTable<ApiActivityLog>();

            var query =
                from a in activityLog
                where a.id == id
                select a;
            
            context.Delete(query.Single());
        }

        /// <summary>
        /// Registro registro de actividad
        /// </summary>
        /// <param name="moduleId">Identificador del modulo</param>
        /// <param name="status">Estado del registro</param>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="stackTrace">Pila de llamadas</param>
        /// <returns>Registro ActivityLog</returns>
        public ApiActivityLog Post(ApiActivityLog.Target target, ApiActivityLog.RequestType requestType, string resource, string reqHeaders, string reqBody, string respHeaders, string respBody, int status)
        {
            ApiActivityLog activityLog = new ApiActivityLog() {
                target = target,
                status = status,
                requestType = requestType,
                resource = resource,
                reqHeaders = reqHeaders,
                reqBody = reqBody,
                respHeaders = respHeaders,
                respBody = respBody
            };                        
            activityLog.id = Insert(activityLog);

            return activityLog;
        }        
    }
}
