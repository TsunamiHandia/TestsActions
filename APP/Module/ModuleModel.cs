using APP.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP
{ 
    //TODO  En futuro lo vamos a mover al proyecto de API
    public class ModuleModel
    {
        
        public IList<InfoModule> value { get; set; }
    }

    public class InfoModule
    {
        public Guid moduleCode { get; set; }        
        public string terminalCode { get; set; }
        public bool moduleActive { get; set; }
        public string moduleName { get; set; }
        public string terminalGUID { get; set; }
        
        public ModuleType _moduleType;
        public ModuleType moduleType
        {
            get
            {
                return _moduleType;
            }
            set
            {
                _moduleType = (ModuleType)Enum.Parse(typeof(ModuleType), value.ToString());
            }
        }


    }
}
