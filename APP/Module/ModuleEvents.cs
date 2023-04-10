using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace APP
{
    /// <summary>
    /// clase que define el evento para que cada modulo se suscriba a él 
    /// </summary>
    public class ModuleEvents
    {
        public static event EventHandler<Dictionary<string,ModuleEvent>> ModuloEvent;
        /// <summary>
        /// metodo para lanzar manualmente el evento y notificar a los modulos suscritos
        /// </summary>
        /// <param name="e">listado que se comparte por cada modulo suscrito, cada uno de ellos se incorpora al listado</param>
        public static void OnModuloEventReached(Dictionary<string, ModuleEvent> e)
        {
            ModuloEvent?.Invoke(new object(), e);

        }
    }
    /// <summary>
    /// clase q define la estructura de un modulo
    /// </summary>
    public class ModuleEvent : EventArgs
    {
        public Module.Module module { get; set; }
    }
}
