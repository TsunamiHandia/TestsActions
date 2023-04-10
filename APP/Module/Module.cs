using APP.Controller;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using static APP.Controller.ApiActivityLogController;

namespace APP.Module
{
    public abstract class Module
    {                
        /// <summary>
        /// Id del modulo
        /// </summary>
        public Guid id;

        /// <summary>
        /// Nombre del modulo
        /// </summary>
        public string name;

        /// <summary>
        /// Tipo del modulo
        /// </summary>
        public ModuleType type;

        /// <summary>
        /// Indica si modulo está activo en la APP
        /// </summary>
        public bool active = true;

        /// <summary>
        /// Indica si modulo registra actividades en el log
        /// </summary>
        public bool registerLog = true;

        /// <summary>
        /// Id min de configuracion
        /// </summary>
        public int minConfigId;

        /// <summary>
        /// Id max de configuracion
        /// </summary>
        public int maxConfigId;

        /// <summary>
        /// fecha de publicacion
        /// </summary>
        public DateTime? addedAt;

        /// <summary>
        /// Inicializa modulo
        /// </summary>
        public abstract void up();

        /// <summary>
        /// Inicializa base de datos
        /// </summary>
        public abstract void initDB(SQL sql, bool recreate, bool seed);

        /// <summary>
        /// Boton en el menu lateral de WPF
        /// </summary>
        /// <returns>ButtonDecor</returns>
        public abstract ButtonDecor getDecorButton();              

        /// <summary>
        /// Indica si modulo está activo en remoto Cloud/OnPremise
        /// </summary>        
        public bool isActiveRemote() {              
            ODataV4Controller api = new ODataV4Controller();
            ConfigController configCtrl = new ConfigController();
            string company = configCtrl.GetValue(ConfigId.APP_COMPANY_ID);
            string idTerminal = configCtrl.GetValue(ConfigId.APP_ID);

            string resource = "/api/liderIT/tc/v1.0/modulesTerminalInfo";
            resource += ODataV4Controller.AddQueryParam("company", configCtrl.GetValue(ConfigId.APP_COMPANY_ID), true);
            resource += ODataV4Controller.AddQueryParam("$filter", $"terminalGUID eq {configCtrl.GetValue(ConfigId.APP_ID)}");                

            HttpResponseMessage response = api.request(ODataV4Controller.requestType.GET, resource);
            if (response == null || !response.IsSuccessStatusCode )
            {
                return false;
            }
            string resultJson = response.Content.ReadAsStringAsync().Result;
            if (!String.IsNullOrEmpty(resultJson))
            {
                ModuleModel modulos = JsonConvert.DeserializeObject<ModuleModel>(resultJson);
                foreach (InfoModule module in modulos.value)
                {                                               
                    if (module.terminalGUID.Equals(idTerminal) && module.moduleActive 
                        && module._moduleType == type && module.moduleCode == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Comprueba si modulo funciona correctamente
        /// </summary>
        /// <returns>Estado del modulo</returns>
        public abstract bool HealthCheck();        

    }
    /// <summary>
    /// Tipos de modulo
    /// Debe tener los mismos valores que en Business Central
    /// </summary>
    public enum ModuleType
    {
        WEIGHING_MACHINE = 1,
        FILE = 2,
        PRINTER = 3,
        REPEATER = 4,
        FTP = 5,          
        
    }

    /// <summary>
    /// Clase con modulos almacenados en la memoria
    /// </summary>
    public class StoreModules
    {

        public static Dictionary<string, ModuleEvent> modulesCollection = new Dictionary<string, ModuleEvent>();

        /// <summary>
        /// Recupera modulo de memoria por Guid
        /// </summary>
        /// <param name="id">Guid del modulo</param>
        /// <returns>Objeto del modulo</returns>
        public static Module GetModule(Guid id)
        {
            ModuleEvent moduleEvent = StoreModules.modulesCollection.Where(a => a.Key == id.ToString())
                                            .Select(a => a.Value)
                                            .FirstOrDefault();

            if (moduleEvent == null)
                return null;

            return moduleEvent.module;
            
        }

        /// <summary>
        /// Recupera modulos activos
        /// </summary>
        /// <returns></returns>
        public static List<Module> GetActiveModules()
        {
            return StoreModules.modulesCollection.Select(a => a.Value.module)
                                                    .Where(b => b.active == true).ToList();
        }
    }

    /// <summary>
    /// Clase con estilo del boton lateral
    /// </summary>
    public class ButtonDecor
    {
        public string Icon { get; set; }
    }

}
