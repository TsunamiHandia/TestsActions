using APP.Controller;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScaleModule {
    public class ScaleTcpIpManager : ScaleManager {
        private TcpClient tcpClient;
        private string Ip;
        private int Port;

        public ScaleTcpIpManager(string IpParam, string PortParam) {

            if (String.IsNullOrEmpty(IpParam))
                throw new ArgumentNullException(nameof(IpParam), "Valor nulo de dirección ip");

            if (String.IsNullOrEmpty(PortParam))
                throw new ArgumentNullException(nameof(PortParam), "Valor nulo de puerto");

            MessageQueue = new ConcurrentQueue<byte[]>();            
            MessageWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            Ip = IpParam;
            int.TryParse(PortParam, out Port);
            Encoding = Encoding.ASCII;
        }

        /// <summary>
        /// Abre puerto serie
        /// </summary>
        /// <returns>Indica si puerto está abierto</returns>
        public override ConnectionStatus OpenPort() {
            ConnectionStatus returnValue = ConnectionStatus.CONECTANDO;
            isConnectionOpen = ConnectionStatus.CONECTANDO;

            try {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto TCP/IP", $"{Ip}:{Port}", "abrir"));

                int limitToTake = GetLimitToTake();
                                
                if (tcpClient != null && IsConnected(tcpClient.Client))
                    tcpClient.Close();

                // Connectamos el TCP Client
                Task connectToPortTask = Task.Factory.StartNew(() => {
                    try {
                        Thread.Sleep(500);
                        tcpClient = new TcpClient(Ip, Port);
                    } catch (Exception ex) when (
                        ex is ArgumentException ||
                        ex is ArgumentOutOfRangeException ||
                        ex is SocketException) {
                            ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR,
                                        String.Format(errorGenericIO, "Puerto TCP/IP", $"{Ip}:{Port}", ex.Message));
                            }
                    });

                connectToPortTask.ContinueWith(t => { 
                    if (tcpClient != null) {

                        NetworkStream stream = tcpClient.GetStream();

                        // Si está establecido un comando para la pesada se envía a la báscula
                        string weighCommand = GetWeighCommand();

                        if (!String.IsNullOrEmpty(weighCommand))
                            Send($"{weighCommand}{System.Environment.NewLine}");

                        Task readTask = Task.Factory.StartNew(() => {
                            byte[] buffer;

                            do {
                                Thread.Sleep(500 * limitToTake);

                                if (tcpClient != null && !IsConnected(tcpClient.Client))
                                    break;

                                buffer = new byte[BUFFER_SIZE];

                                int bytesReceived = stream.Read(buffer, 0, buffer.Length);

                                MessageQueue.Enqueue(buffer);
                                MessageWaitHandle.Set();

                                if (((Module)APP.Module.StoreModules.GetModule(ModuleId)).RegisterCOMStatusLog)
                                    ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, Encoding.GetString(buffer));

                                // Quitar elementos de la cola que excedan el tamaño máximo (Número de capturas)
                                MessageQueueSizeControl();
                            } while (true);
                        });

                        readTask.ContinueWith(completedTask => {
                            ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(errorPerdidaConexion, $"{Ip}:{Port}"));
                            isConnectionOpen = ConnectionStatus.INACTIVO;
                        });

                        ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto TCP/IP", $"{Ip}:{Port}", "abrió"));

                        isConnectionOpen = ConnectionStatus.ACTIVO;
                    } else
                        isConnectionOpen = ConnectionStatus.INACTIVO;                
                });

            } catch (IOException ex) {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(errorGenericIO, "Puerto TCP/IP", $"{Ip}:{Port}", ex.Message));
                returnValue = ConnectionStatus.INACTIVO;
                isConnectionOpen = ConnectionStatus.INACTIVO;
            } catch (Exception ex) {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, $"Error: {ex.Message}");
                returnValue = ConnectionStatus.INACTIVO;
                isConnectionOpen = ConnectionStatus.INACTIVO;
            }

            isConnectionOpen = returnValue;

            return returnValue;
        }

        /// <summary>
        /// Comprueba el estado de la conexión TCP/IP
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private bool IsConnected(Socket socket) {
            bool returnValue = false;

            if (socket != null)
                try {
                    returnValue = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
                } catch {}

            return returnValue;
        }

        /// <summary>
        /// Obtiene el contenido escrito en el puerto serial
        /// </summary        
        /// <returns>String con mensaje</returns>      
        public override string GetMessage() {
            string returnValue = string.Empty;

            ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto TCP/IP", $"{Ip}:{Port}", "recuperar mensaje"));
            
            returnValue = GetMessageFromMessageQueue();

            ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto TCP/IP", $"{Ip}:{Port}", $"recuperó mensaje ({returnValue})"));

            return returnValue;
        }

        /// <summary>
        ///Enviar mensaje (string)
        /// </summary>
        ///< param name = "encoding" > metodo de codificación de texto See < see CREF = "encoding" / > < / param >
        /// <param name="message"></param>
        public override ScaleManager Send(string message) {
            try {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto TCP/IP", $"{Ip}:{Port}", $"enviar comando ({message})"));
                                
                var buffer = Encoding.GetBytes(FormatMessage(message));
                NetworkStream stream = tcpClient.GetStream();
                stream.Write(buffer, 0, buffer.Length);

                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto TCP/IP", $"{Ip}:{Port}", $"envío comando ({message})"));
            } catch (Exception ex) when (
                    ex is ArgumentNullException ||
                    ex is InvalidOperationException ||
                    ex is ArgumentOutOfRangeException ||
                    ex is TimeoutException ||
                    ex is ArgumentException) {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(error, "Puerto TCP/IP", $"{Ip}:{Port}", $"escribir texto ({message})"));
            }

            return this;
        }

        /// <summary>
        /// Espera respuesta durante X milisengos
        /// </summary>
        /// <param name="milliseconds">Milisegundos de espera</param>
        /// <returns></returns>
        public override ScaleManager Wait(int milliseconds) {
            Thread.Sleep(milliseconds);
            return this;
        }

        /// <summary>
        /// Indica si puerto TCP/IP esta abierto
        /// </summary>
        /// <returns></returns>
        public override ConnectionStatus IsOpen() {
            return tcpClient != null && IsConnected(tcpClient.Client) ? ConnectionStatus.ACTIVO : ConnectionStatus.INACTIVO;
        }

        /// <summary>
        /// Cierre del puerto TCP/IP
        /// </summary>
        public override void Close() {
            try {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto TCP/IP", $"{Ip}:{Port}", "cerrar"));

                if (tcpClient != null)
                    tcpClient.Close();

                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto TCP/IP", $"{Ip}:{Port}", "cerró"));
            } catch (Exception) {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(error, "Puerto TCP/IP", $"{Ip}:{Port}", "cerrar"));
            }
        }
    }
}
