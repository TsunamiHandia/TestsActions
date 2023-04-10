using System;
using System.IO;
using System.Windows.Threading;
using APP;
using APP.Controller;
using Newtonsoft.Json;
using FileModule.API;

namespace FileModule
{/// <summary>
 /// Clase que se encarga de levantar un proceso que sincroniza ficheros de una ruta con Business Central OnCloud/OnPrem
 /// </summary>
    public class FileManager
    {
        //check files into directory
        private DispatcherTimer dispatcherTimer;
        private ConfigController configCtrl;
        private ActivityLogController activityLogCtrl;
        private string origin;
        private string filterExt;
        private string destination;
        private string idTeminalGUID;
        private bool sincroSubFolders;
        private Byte[] bytes;
        private string fileText;
        private FileModel fichero;
        string ficheroJson;
        private string[] files;
        private Operacion operation;
        private string companyOnSandBox;
        private ODataV4Controller api;
        private int seconds;
        private string fileName = null;
        private string destFile = null;
        private Guid IdModule;
        private static string tryAction = "Intento de {2} el {0} {1}";
        private static string succes = "Se {2} correctamente el {0} {1}";
        private static string error = "Error al {2} el {0} {1}";
        private static string errorGenericIO = "Error de entrada/salida con el {0} {1}";
        private static string dontExist = "Error, el {0} {1} no existe";
        private static string directoryLabel = "directorio";
        private static string fileLabel = "archivo";
        private const string endPointPostFile = "/ODataV4/TCService_CargarFichero?Company={0}";
        /// <summary>
        /// procedimiento que se encarga de inicializar el subproceso de sincronizacion
        /// </summary>
        /// <param name="source">ruta de origen</param>
        /// <param name="d">ruta destino en caso de que se requiera mover el fichero</param>
        /// <param name="op">tipo de operacion (Eliminar o Mover)</param>
        /// <param name="frecuence">intervalo de tiempo entre una sincronización y otra</param>
        public FileManager(string source, string d, Operacion op, bool goDeeper, Guid idModule, string pattern)
        {
            origin = source;
            destination = d;
            filterExt = pattern;
            operation = op;
            IdModule = idModule;
            sincroSubFolders = goDeeper;
        }

        public void Run(string frecuence)
        {
            int.TryParse(frecuence, out seconds);
            dispatcherTimer = new DispatcherTimer();
            configCtrl = new ConfigController();
            api = new ODataV4Controller();
            companyOnSandBox = configCtrl.GetValue(ConfigId.APP_COMPANY_ID);
            activityLogCtrl = new ActivityLogController();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, seconds);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// evento donde ocurre la llamada cada x tiempo para sincronizar nuevamente
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            sincroFiles(origin, destination, operation, sincroSubFolders, IdModule, filterExt);

        }
        /// <summary>
        /// procedimiento que se encarga de leer desde una ruta y realizar la operacion correspondiente con los ficheros
        /// </summary>
        /// <param name="source">ruta de origen</param>
        /// <param name="destination">ruta destino (si es necesaria)</param>
        /// <param name="operation">operacion a realizar (Eliminar/Mover)</param>
        public void sincroFiles(string source, string destination, Operacion operation, bool sincroSubFolders, Guid IdModule, string filterExt)
        {
            idTeminalGUID = configCtrl.GetValue(ConfigId.APP_ID);
            try
            {
                if (!Directory.Exists(source))
                {
                    activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.ERROR, String.Format(dontExist, directoryLabel, source));
                    return;
                }

                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, directoryLabel, source, "leer"));


                files = sincroSubFolders ? Directory.GetFiles(@source, filterExt, SearchOption.AllDirectories) : Directory.GetFiles(@source, filterExt, SearchOption.TopDirectoryOnly);

                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(succes, directoryLabel, source, "leyó"));

                foreach (string file in files)
                {
                    if (operation != Operacion.DO_NOTHING)
                    {
                        fileName = Path.GetFileName(file);
                        destFile = Path.Combine(destination, fileName);

                        activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, fileLabel, file, "leer"));

                        //try to send file
                        bytes = File.ReadAllBytes(file);
                        fileText = Convert.ToBase64String(bytes);
                        fichero = new FileModel(fileText, fileName, idTeminalGUID, file, IdModule);
                        ficheroJson = JsonConvert.SerializeObject(fichero);

                        activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, fileLabel, file, "enviar"));

                        if (SendSuccessfully(ficheroJson))
                        {
                            activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, fileLabel, file, "envió"));

                            if (operation == Operacion.MOVE_FILE)
                            {
                                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, fileLabel, file, "mover"));

                                if (!Directory.Exists(destination))
                                {
                                    activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.ERROR, String.Format(dontExist, directoryLabel, destination));
                                    return;
                                }

                                File.Move(file, destFile, true);

                                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(succes, fileLabel, file, $"movió a {destFile}"));

                            }
                            else if (operation == Operacion.DELETE_FILE)
                            {
                                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, fileLabel, file, "eliminar"));

                                File.Delete(file);
                                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.INFO, String.Format(succes, fileLabel, file, "eliminó"));

                            }
                        }
                        else
                        {
                            activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.ERROR, String.Format(error, fileLabel, file, "enviar"));

                        }

                    }


                }

            }

            catch (FileNotFoundException)
            {
                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.ERROR, String.Format(dontExist, fileLabel, destFile));

            }
            catch (IOException)
            {
                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.ERROR, String.Format(errorGenericIO, fileLabel, destFile));

            }
            catch (Exception ex)
            {
                activityLogCtrl.Post(IdModule, ActivityLogController.ActivityLog.Status.ERROR, ex.Message, ex.StackTrace);

            }

        }
        /// <summary>
        /// procedimiento que intenta subir el fichero a business central
        /// </summary>
        /// <param name="fileJson">objeto fichero en formato Json</param>
        /// <returns></returns>
        private bool SendSuccessfully(string fileJson)
        {
            var result = api.request(ODataV4Controller.requestType.POST, String.Format(endPointPostFile, companyOnSandBox), fileJson);
            return (result.IsSuccessStatusCode);
        }
        /// <summary>
        /// retorna el subproceso para acceder a él desde otro sitio
        /// </summary>
        /// <returns></returns>
        public DispatcherTimer getDispatcherTimer()
        {
            return dispatcherTimer;
        }
        /// <summary>
        /// modifica el tipo de operacion a realizar
        /// </summary>
        /// <param name="op">tipo de operacion Eliminar/Mover</param>
        public void setOperation(Operacion op)
        {
            operation = op;
        }
        /// <summary>
        /// Modifica la ruta de origen donde debe leer el proceso de sincronizacion
        /// </summary>
        /// <param name="source">nueva ruta de origen</param>
        public void setOrigin(string source)
        {
            origin = source;
        }
        /// <summary>
        /// Modifica el patrón de búsqueda de los ficheros
        /// </summary>
        /// <param name="source">nueva ruta de origen</param>
        public void setSearchPattern(string pattern)
        {
            filterExt = pattern;

        }
        /// <summary>
        /// Modifica la ruta destino donde debe enviar el proceso de sincronizacion
        /// </summary>
        /// <param name="dest">nueva ruta destino</param>
        public void setDestination(string dest)
        {
            destination = dest;
        }

        internal void setSincroSubFolders(bool goDeeper)
        {
            sincroSubFolders = goDeeper;
        }
    }
}
