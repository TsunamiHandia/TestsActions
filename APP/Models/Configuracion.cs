using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;

namespace APP.Models
{
    public class Configuracion
    {
        public long Id { get; set; }
        public string TenantId { get; set; }
        public int Entorno { get; set; }
    }
}
