using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepeaterModule.API
{
    public class RepeaterResponse
    {
        /// <summary>
        /// Estado de la respuesta: 200,500,etc
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// Cabeceras de la respuesta
        /// </summary>
        public List<RepeaterHeader> headers { get; set; }
        /// <summary>
        /// Cuerpo de la respuesta
        /// </summary>
        public string body { get; set; }
    }
}
