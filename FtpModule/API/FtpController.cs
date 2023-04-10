using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APP;
using APP.Controller;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using FTPModule;
using System.Drawing.Printing;
using System.Net;
using FluentFTP;

namespace API
{
    [ApiController]
    public class FtpController : ControllerBase
    {
        /// <summary>
        /// Lista el contenido de un directorio ftp
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("/api/ftp/list")]
        [HttpPost]
        public IEnumerable<FtpListItem> Get([FromBody] FtpFileModel value)
        {
            Module module = (Module)APP.Module.StoreModules.GetModule(value.moduleId);
            if (module == null)
                throw new Exception("No existe modulo");

            module.up();

            IEnumerable<FtpListItem> FileList = module.GetFtpManager().ListFiles(value);

            return FileList;

        }

        /// <summary>
        /// Crear ficheros
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("/api/ftp/createDirectory")]
        [HttpPost]
        public string PostCreateDirectory(string action, [FromBody] FtpFileModel value)
        {
            Module module = (Module)APP.Module.StoreModules.GetModule(value.moduleId);
            string responseTxt = String.Empty;
            FtpStatus status = FtpStatus.Failed;
            if (module == null)
                throw new Exception("No existe modulo");

            module.up();
            status = module.GetFtpManager().CreateDirectories(value);

            return status.ToString();
        }

        [Route("/api/ftp/upload")]
        [HttpPost]
        public string PostUpload(string action, [FromBody] FtpFileModel value)
        {
            Module module = (Module)APP.Module.StoreModules.GetModule(value.moduleId);
            FtpStatus status = FtpStatus.Failed;
            if (module == null)
                throw new Exception("No existe modulo");
            module.getDecorButton();
            module.HealthCheck();
            module.up();
            status = module.GetFtpManager().UploadFiles(value);
            return status.ToString();

        }

        /// <summary>
        /// Descarga un fichero desde un ftp
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("/api/ftp/download")]
        [HttpPost]
        public FTPClass PostDownload([FromBody] FtpFileModel value)
        {
            FTPClass FtpFile = new FTPClass();
            Module module = (Module)APP.Module.StoreModules.GetModule(value.moduleId);

            if (module == null)
                throw new Exception("No existe modulo");

            module.up();

            string base64String = module.GetFtpManager().DownLoadFile(value);
            FtpFile.base64 = base64String;
            return FtpFile;

        }

        /// <summary>
        /// Elimina varios ficheros en distintas rutas
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("/api/ftp/delete")]
        [HttpPost]
        public string Delete([FromBody] FtpFileModel value)
        {
            Module module = (Module)APP.Module.StoreModules.GetModule(value.moduleId);

            if (module == null)
                throw new Exception("No existe modulo");

            module.up();
            FtpStatus statusCode = module.GetFtpManager().DeleteItems(value);
            return statusCode.ToString();

        }

        public class FTPClass
        {
            public string base64;
        }

        public class FtpFileClass
        {
            public string type;
            public string file;
        }
        public class FtpFiles
        {
            public string name;
            public string base64;
            public string targetPath;

            public FtpFiles(string name, string base64, string targetPath)
            {
                this.name = name;
                this.base64 = base64;
                this.targetPath = targetPath;
            }
        }
        public class FtpDirs
        {
            public string name;
            public string targetPath;
            public FtpDirs(string name, string targetPath)
            {
                this.name = name;
                this.targetPath = targetPath;
            }
        }
        public class FtpItem
        {
            public string type;
            public string name;

            public FtpItem(string type, string name)
            {
                this.type = type;
                this.name = name;
            }
        }

        // TODO hay varias peticiones usando este modelo donde cada una usa los campos q necesita
        // si se quiere controlar de otra forma tendriamos q crear una clase por cada estructura del body de la peticion
        public class FtpFileModel
        {
            public Guid moduleId;
            public string url;
            public int port;
            public bool validateCertificate;
            public string user;
            public string pass;
            public string path;
            public IEnumerable<FtpFiles> files;
            public IEnumerable<FtpDirs> directories;
            public IEnumerable<FtpItem> items;
        }
    }
}