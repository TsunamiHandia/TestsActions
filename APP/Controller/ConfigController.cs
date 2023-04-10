using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using LinqToDB;
using LinqToDB.Mapping;

namespace APP.Controller
{        
    public class ConfigController : Controller, IConfigController
    {
        [Table(Name = "Config")]
        public class Config
        {
            [Column(Name = "Id", IsPrimaryKey = true, CanBeNull = false)]
            public int id { get; set; }          

            [Column(Name = "Value", CanBeNull = false)]
            public string value { get; set; }

            [Column(Name = "ModuleId", IsPrimaryKey = true, CanBeNull = true, DataType = DataType.Text)]
            public Guid? moduleId { get; set; }

            [Column(Name = "AddedAt")]
            public DateTime? addedAt { get; set; }

            [Column(Name = "UpdatedAt")]
            public DateTime? updatedAt { get; set; }

        }

        public ConfigController()
        {

        }
        public ConfigController(SQL sql) : base(sql)
        {

        }

        /// <summary>
        /// Recuperar registro por id
        /// </summary>
        /// <param name="id">Id de registro configuracion</param>
        /// <param name="moduleId">(Opcional) Id del modulo</param>
        /// <returns>Registro Config</returns>
        public Config Get(Enum id, System.Guid? moduleId = null)
        {
            DataContext context = this.getDataContext();
            ITable<Config> config = context.GetTable<Config>();

            if(moduleId == null)
            {
                moduleId = Guid.Empty;
            }

            var query =
                from a in config
                where a.id == Convert.ToInt32(id) && a.moduleId == moduleId 
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
        /// <returns>Listado de Config</returns>
        public List<Config> List(int limit)
        {
            DataContext context = this.getDataContext();
            ITable<Config> config = context.GetTable<Config>();

            var query =                
                from a in config   
                orderby a.id descending
                select a;

            return query.Take(limit).ToList();                  
        }

        /// <summary>
        /// Crea un nuevo registro de Config
        /// </summary>
        /// <param name="config">Registro Config</param>
        private void Insert(Config configInsert)
        {                        
            DataContext context = this.getDataContext();                                              
            ITable<Config> config = context.GetTable<Config>();
            configInsert.addedAt = DateTime.Now;
            configInsert.updatedAt = DateTime.Now;
            context.Insert(configInsert);
        }

        /// <summary>
        /// Modifica registro existente de Config
        /// </summary>
        /// <param name="config">Registro Config</param>
        /// <param name="moduleId">(Opcional) Id del modulo</param>
        private void Update(Config configUpdate, System.Guid? moduleId)
        {
            DataContext context = this.getDataContext();
            ITable<Config> config = context.GetTable<Config>();

            var query =
                from a in config
                where a.id == configUpdate.id && a.moduleId == moduleId
                select a;

            Config configDb = query.Single();
            configDb.value = configUpdate.value;
            configDb.updatedAt = DateTime.Now;
            context.Update(configDb);
        }

        /// <summary>
        /// Elimina registro existente buscando por id
        /// </summary>
        /// <param name="id">Id de registro configuracion</param>
        /// <param name="moduleId">(Opcional) Id del modulo</param>
        private void Delete(Enum id, Guid? moduleId = null)
        {
            DataContext context = this.getDataContext();
            ITable<Config> config = context.GetTable<Config>();
            if(moduleId == null)
            {
                moduleId = Guid.Empty;
            }
            var query =
                from a in config
                where a.id == Convert.ToInt32(id) && a.moduleId == moduleId
                select a;
            
            context.Delete(query.Single());
        }

        /// <summary>
        /// Almacena configuracion en APP
        /// </summary>
        /// <param name="id">Id de registro configuracion</param>
        /// <param name="value">Nuevo valor</param>        
        /// <param name="moduleId">(Opcional) GUID del modulo</param>        
        /// <returns>Registro Config</returns>
        public Config Save(Enum id, string value, Guid? moduleId = null)
        {
            if(value == null)
            {
                throw new FormatException($"{id} no puede ser nulo");
            }
            if (value.Length > 2048)
            {
                throw new FormatException($"La longitud de {id} en config no puede ser superior a 1024 caracteres");
            }
            if(moduleId == null)
            {
                moduleId = Guid.Empty;
            }
            Config config = this.Get(id, moduleId);
            if(config != null)
            {
                config.value = value;
                this.Update(config, (Guid)moduleId);
            }
            else
            {
                config = new Config();
                config.id = Convert.ToInt32(id);
                config.value = value;
                config.moduleId = moduleId;
                this.Insert(config);
            }
            return config;
        }

        /// <summary>
        /// Elimina config
        /// </summary>
        /// <param name="id">Id de configuracion</param>
        /// <param name="moduleId">(Opcional) Id del modulo</param>
        /// <returns>Indica si ha borrado correctamente Config</returns>
        public bool Remove(Enum id, Guid? moduleId = null)
        {
            if(moduleId == null)
            {
                moduleId = Guid.Empty;
            }
            Config config = this.Get(id,moduleId);
            if (config == null)
            {
                return false;
            }
            this.Delete(id,moduleId);
            return true;
        }

        /// <summary>
        /// Recupera valor de config
        /// </summary>
        /// <param name="id">Id de configuracion</param>
        /// <returns>Valor de config</returns>
        public string GetValue(Enum id, Guid? moduleId = null)
        {
            if(moduleId == null)
            {
                moduleId = Guid.Empty;
            }
            Config config = Get(id, moduleId);
            if(config!= null)
            {
                return config.value;
            }
            return String.Empty;
        }

    }
}
