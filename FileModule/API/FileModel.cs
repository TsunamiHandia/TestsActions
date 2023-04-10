using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileModule.API
{
    public class FileModel
    {
        public FileModel(string f, string nombFichero, string idTerm, string ruta, Guid moduleId) {
            fichero = f;
            nombreFichero = nombFichero;
            idTerminal = idTerm;
            rutaOrigen = ruta;
            this.moduleId = moduleId;
        }

        public string fichero { get; set; }
        public string nombreFichero { get; set; }
        public string idTerminal { get; set; }
        public string rutaOrigen { get; set; }
        public Guid moduleId { get; set; }
    }
}
