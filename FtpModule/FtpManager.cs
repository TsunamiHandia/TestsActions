using API;
using APP.Controller;
using FluentFTP;
using FluentFTP.Client.BaseClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace FTPModule
{
    public class FtpManager
    {
        private Guid ModuleId;
        private static string message = "{0} al {1} {2} {3} en el recurso {4}";

        public FtpManager(Guid ModuleId)
        {
            this.ModuleId = ModuleId;
        }


        /// <summary>
        /// Descarga de un fichero de ftp
        /// </summary>
        /// <param name="fileModel"></param>
        /// <returns>base64 string</returns>
        public string DownLoadFile(FtpController.FtpFileModel fileModel)
        {
            FtpStatus status = FtpStatus.Failed;
            ActivityLogController activityLogCtrl = new ActivityLogController();
            string base64String = String.Empty;
            string fileName = String.Empty;
            try
            {
                FtpClient client = new FtpClient(fileModel.url, fileModel.user, fileModel.pass);
                //si trata de conectar de forma segura
                if (fileModel.validateCertificate)
                {
                    client.Config.EncryptionMode = FtpEncryptionMode.Auto;
                    client.Config.SslProtocols = SslProtocols.Tls12;
                    client.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                }
                client.Port = fileModel.port;
                client.Connect();

                String[] values = fileModel.path.Split('/');
                fileName = values[values.Length - 1];
                string pathString = Path.Combine(Path.GetTempPath(), fileName);
                if (!client.FileExists(fileModel.path))
                {
                    throw new Exception(String.Format("Error, el fichero {0} no existe en la ruta {1}", fileName, fileModel.path));
                }
                status = client.DownloadFile(pathString, fileModel.path);
                Byte[] bytes = System.IO.File.ReadAllBytes(pathString);
                base64String = Convert.ToBase64String(bytes);
                activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(message, "Éxito", "descargar", "fichero", fileName, fileModel.url));
                status = FtpStatus.Success;
            }
            catch (Exception ex)
            {
                activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(message, "Error", "descargar", "fichero", fileName, fileModel.url), ex.StackTrace);
                status = FtpStatus.Failed;
                throw new Exception(ex.Message);
            }

            return base64String;

        }

        /// <summary>
        /// Listado de los elementos de un directorio ftp
        /// </summary>
        /// <param name="fileModel"></param>
        /// <returns></returns>
        public IEnumerable<FtpListItem> ListFiles(FtpController.FtpFileModel fileModel)
        {

            List<FtpListItem> items;
            ActivityLogController activityLogCtrl = new ActivityLogController();
            try
            {
                FtpClient client = new FtpClient(fileModel.url, fileModel.user, fileModel.pass);
                //si trata de conectar de forma segura
                if (fileModel.validateCertificate)
                { 
                    client.Config.EncryptionMode = FtpEncryptionMode.Auto;
                    client.Config.SslProtocols = SslProtocols.Tls12;
                    client.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);                    
                }
                client.Port = fileModel.port;
                client.Connect();                
                
                if (!client.DirectoryExists(fileModel.path))
                {
                    throw new Exception(String.Format("Error, la ruta {0} no existe", fileModel.path));
                }
                items = new List<FtpListItem>();
                foreach (FtpListItem item in client.GetListing(fileModel.path))
                {
                    // if this is a file
                    if (item.Type == FtpObjectType.File)
                    {
                        long size = client.GetFileSize(item.FullName);
                    }

                    // get modified date/time of the file or folder
                    DateTime time = client.GetModifiedTime(item.FullName);
                    items.Add(item);
                }
            }

            catch (Exception ex)
            {
                activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(message, "Error", "listar", "documentos", "", fileModel.url), ex.StackTrace);
                throw new Exception(ex.Message);
            }
            activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(message, "Éxito", "listar", "documentos", "", fileModel.url));
            return items;
        }

        private void OnValidateCertificate(BaseFtpClient control, FtpSslValidationEventArgs e)
        {
            // toda la logica que necesitemos para validar el ceritifcado va aqui
            e.Accept = true;
        }

        /// <summary>
        /// Crea nuevos ficheros en distintas rutas
        /// </summary>
        /// <param name="fileModel"></param>
        /// <returns></returns>
        public FtpStatus UploadFiles(FtpController.FtpFileModel fileModel)
    {
        FtpStatus status = FtpStatus.Failed;
        string itemName = String.Empty;
        ActivityLogController activityLogCtrl = new ActivityLogController();
        try
        {
            FtpClient client = new FtpClient(fileModel.url, fileModel.user, fileModel.pass);
                //si trata de conectar de forma segura
                if (fileModel.validateCertificate)
                {
                    client.Config.EncryptionMode = FtpEncryptionMode.Auto;
                    client.Config.SslProtocols = SslProtocols.Tls12;
                    client.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                }
                client.Port = fileModel.port;
                client.Connect();

                foreach (var item in fileModel.files)
            {
                string pathString = Path.Combine(Path.GetTempPath(), item.name);
                Byte[] bytes = Convert.FromBase64String(item.base64);
                System.IO.File.WriteAllBytes(pathString, bytes);
                itemName = item.name;
                string From = pathString;
                string To = String.Format("/{0}/{1}", item.targetPath, item.name);
                status = client.UploadFile(pathString, To, FtpRemoteExists.Overwrite, true);
                activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(message, "Éxito", "crear", "fichero", item.name, fileModel.url));                
            }           

        }
        catch (Exception ex)
        {
          activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(message, "Error", "crear", "fichero", itemName, fileModel.url), ex.StackTrace);
          status = FtpStatus.Failed;
          throw new Exception(ex.Message);
        }

        return status;
    }

    /// <summary>
    /// crea directorios en distintas rutas
    /// </summary>
    /// <param name="fileModel">objeto con los parametros de conexion</param>
    /// <returns></returns>
    public FtpStatus CreateDirectories(FtpController.FtpFileModel fileModel)
    {
            FtpStatus status = FtpStatus.Failed;
            string itemName = String.Empty;
            ActivityLogController activityLogCtrl = new ActivityLogController();
            try
            {
                FtpClient client = new FtpClient(fileModel.url, fileModel.user, fileModel.pass);
                //si trata de conectar de forma segura
                if (fileModel.validateCertificate)
                {
                    client.Config.EncryptionMode = FtpEncryptionMode.Auto;
                    client.Config.SslProtocols = SslProtocols.Tls12;
                    client.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                }
                client.Port = fileModel.port;
                client.Connect();

                foreach (var item in fileModel.directories)
                {
                    itemName = item.name;
                    if (client.CreateDirectory(item.targetPath + "/" + item.name, true))
                    {
                        status = FtpStatus.Success;
                        activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(message, "Éxito", "crear", "directorio", itemName, fileModel.url));
                    }
                    else { 
                        status = FtpStatus.Skipped; // ya existe el directorio
                        activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(message, "Error", "crear", "directorio", itemName, fileModel.url) + " ya existe");
                        throw new Exception("Error, el directorio ya existe");
                    }                    
                }

            }
            catch (Exception ex)
            {                
                activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(message, "Error", "crear", "directorio", itemName, fileModel.url), ex.StackTrace);
                status = FtpStatus.Failed;
                throw new Exception(ex.Message);
            }

            return status;

        }

        /// <summary>
        /// procedimiento para eliminar multiples directorios y su contenido correspondiente
        /// </summary>
        /// <param name="fileModel">modelo de objeto con los parametros necesarios</param>
        /// <returns>FileActionOk si todo va bien</returns>
        public FtpStatus DeleteItems(FtpController.FtpFileModel fileModel)
        {
            FtpStatus statusCode = FtpStatus.Failed;
            foreach (var item in fileModel.items)
            {
                statusCode = DeleteFTPDirectory(item.name, fileModel.url, fileModel.user, fileModel.pass, fileModel.port, fileModel.validateCertificate);
            }
            return statusCode;
        }

        /// <summary>
        /// procedimiento recursivo para eliminar un directorio y su contenido
        /// </summary>
        /// <param name="Path">ruta al directorio</param>
        /// <param name="ServerAdress">url base</param>
        /// <param name="Login">usuario</param>
        /// <param name="Password">contraseña</param>
        /// <returns>FileOKAction si todo se ha eliminado correctamente</returns>
        public FtpStatus DeleteFTPDirectory(string Path, string ServerAdress, string Login, string Password, int port, bool validate)
        {
            FtpStatus status = FtpStatus.Failed;
            string itemName = Path;
            ActivityLogController activityLogCtrl = new ActivityLogController();
            try
            {
                FtpClient client = new FtpClient(ServerAdress, Login, Password);
                //si trata de conectar de forma segura
                if (validate)
                {
                    client.Config.EncryptionMode = FtpEncryptionMode.Auto;
                    client.Config.SslProtocols = SslProtocols.Tls12;
                    client.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);                    
                }
                client.Port = port;
                client.Connect(); 

                if (!client.DirectoryExists(Path))
                {
                    throw new Exception(String.Format("Error, la ruta {0} no existe",Path));
                }
                client.DeleteDirectory(Path);
                activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(message, "Éxito", "eliminar", "directorio", itemName, ServerAdress));
                status = FtpStatus.Success;
            }
            catch (Exception ex)
            {
                activityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(message, "Error", "crear", "directorio", itemName, ServerAdress), ex.StackTrace);
                status = FtpStatus.Failed;
                throw new Exception(ex.Message);
            }

            return status;

        }

        public bool ExistDirectory(string path, string url, string user, string pass, int port, bool validate)
        {
            try
            {
                FtpClient client = new FtpClient(url, user , pass);
                //si trata de conectar de forma segura
                if (validate)
                {
                    client.Config.EncryptionMode = FtpEncryptionMode.Auto;
                    client.Config.SslProtocols = SslProtocols.Tls12;
                    client.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
                }
                client.Port = port;
                client.Connect();

                if (!client.DirectoryExists(path))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {                
                throw new Exception(ex.Message);
            }
        }
    }
}