using APP.Controller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace ScaleModule {
    public class ScaleComManager : ScaleManager {
        /// Puerto serie        
        private SerialPort SerialPort;
        private string COMPort;
        private int BaudRate;
        private int DataBits;
        private string Parity;
        private string StopBit;
        private string FlowControl;

        /// Lecturas
        private int ReceiveCount, SendCount;
        private readonly object Mux;

        public ScaleComManager(string COMPortParam, string BaudRateParam, string DataBitsParam, string ParityParam,
                            string StopBitParam, string FlowControlParam) {

            if (String.IsNullOrEmpty(COMPortParam))
                throw new ArgumentNullException(nameof(COMPortParam), "Valor nulo de puerto COM");

            if (String.IsNullOrEmpty(BaudRateParam))
                throw new ArgumentNullException(nameof(BaudRateParam), "Valor nulo de bits por segundo");

            if (String.IsNullOrEmpty(DataBitsParam))
                throw new ArgumentNullException(nameof(DataBitsParam), "Valor nulo de bits de datos");

            if (String.IsNullOrEmpty(ParityParam))
                throw new ArgumentNullException(nameof(ParityParam), "Valor nulo de bits de paridad");

            if (String.IsNullOrEmpty(StopBitParam))
                throw new ArgumentNullException(nameof(StopBitParam), "Valor nulo de bits de parada");

            if (String.IsNullOrEmpty(FlowControlParam))
                throw new ArgumentNullException(nameof(FlowControlParam), "Valor nulo de control de flujo");

            Mux = new object();
            ReceiveCount = 0;
            SendCount = 0;
            MessageQueue = new ConcurrentQueue<byte[]>();
            MessageWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            COMPort = COMPortParam;
            int.TryParse(BaudRateParam, out BaudRate);
            int.TryParse(DataBitsParam, out DataBits);
            Parity = ParityParam;
            StopBit = StopBitParam;
            FlowControl = FlowControlParam;
            Encoding = Encoding.Default;
        }

        /// <summary>
        /// Abre puerto serie
        /// </summary>
        /// <returns>Indica si puerto está abierto</returns>
        public override ConnectionStatus OpenPort() {
            ConnectionStatus returnValue = ConnectionStatus.CONECTANDO;
            isConnectionOpen = ConnectionStatus.CONECTANDO;

            try {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto", COMPort, "abrir"));

                if (SerialPort != null && SerialPort.IsOpen)
                    SerialPort.Close();

                // abrimos el puerto
                SerialPort = new SerialPort(COMPort.ToUpper(), BaudRate);
                SerialPort.Parity = (Parity)Enum.Parse(typeof(Parity), Parity, true);
                SerialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBit, true);
                SerialPort.DataBits = DataBits;
                SerialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), FlowControl, true);
                SerialPort.RtsEnable = true;
                SerialPort.Encoding = Encoding;

                // tiempos de espera
                SerialPort.ReadTimeout = 500;
                SerialPort.WriteTimeout = 500;

                // tamaños del buffer
                SerialPort.ReadBufferSize = BUFFER_SIZE * BUFFER_SIZE;
                SerialPort.WriteBufferSize = BUFFER_SIZE * BUFFER_SIZE;

                SerialPort.Open();

                SerialPort.DiscardInBuffer();
                SerialPort.DiscardOutBuffer();

                SerialPort.PinChanged += (sender, e) => {                    
                    SerialPort serialPort = (SerialPort)sender;                    
                    IsOpen();
                };

                SerialPort.DataReceived += (sender, e) => {
                    try {
                        lock (Mux) {
                            int length = SerialPort.BytesToRead;
                            byte[] buffer = new byte[length];
                            SerialPort.Read(buffer, 0, length);
                            ReceiveCount += length;
                            MessageQueue.Enqueue(buffer);
                            MessageWaitHandle.Set();

                            if (((Module)APP.Module.StoreModules.GetModule(ModuleId)).RegisterCOMStatusLog)
                                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, Encoding.GetString(buffer));

                            // Quitar elementos de la cola que excedan el tamaño máximo (Número de capturas)
                            MessageQueueSizeControl();
                        }
                    } catch (Exception ex) when (
                        ex is ArgumentNullException ||
                        ex is InvalidOperationException ||
                        ex is ArgumentOutOfRangeException ||
                        ex is TimeoutException ||
                        ex is ArgumentException
                        ) {

                        if (((Module)APP.Module.StoreModules.GetModule(ModuleId)).RegisterCOMStatusLog) {
                            ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(error, "Puerto", COMPort, "leer"));
                        }
                    }
                };

                SerialPort.ErrorReceived += (sender, e) => {
                    if (((Module)APP.Module.StoreModules.GetModule(ModuleId)).RegisterCOMStatusLog)
                        ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, e.EventType.ToString());
                };

                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto", COMPort, "abrió"));

                returnValue = ConnectionStatus.ACTIVO;
            } catch (IOException ex) {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(errorGenericIO, "Puerto", COMPort, ex.Message));
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
        /// Obtiene el contenido escrito en el puerto serial
        /// </summary        
        /// <returns>String con mensaje</returns>      
        public override string GetMessage() {
            string returnValue = string.Empty;

            ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto", COMPort, "recuperar mensaje"));
            
            returnValue = GetMessageFromMessageQueue();

            ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto", COMPort, $"recuperó mensaje ({returnValue})"));
            
            return returnValue;
        }

        /// <summary>
        ///Enviar mensaje (string)
        /// </summary>
        ///< param name = "encoding" > metodo de codificación de texto See < see CREF = "encoding" / > < / param >
        /// <param name="message"></param>
        public override ScaleManager Send(string message) {
            try {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto", COMPort, $"enviar comando ({message})"));

                lock (Mux) {
                    var buffer = Encoding.GetBytes(FormatMessage(message));
                    SerialPort.Write(buffer, 0, buffer.Length);
                    SendCount += buffer.Length;

                    ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto", COMPort, $"envío comando ({message})"));
                }
            } catch (Exception ex) when (
                        ex is ArgumentNullException ||
                        ex is InvalidOperationException ||
                        ex is ArgumentOutOfRangeException ||
                        ex is TimeoutException ||
                        ex is ArgumentException) {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(error, "Puerto", COMPort, $"escribir texto ({message})"));
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
        /// Indica si puerto seria esta abierto
        /// </summary>
        /// <returns></returns>
        public override ConnectionStatus IsOpen() {
            ConnectionStatus returnValue = ConnectionStatus.INACTIVO;

            if (SerialPort != null)
                returnValue = SerialPort.IsOpen && SerialPort.CtsHolding && SerialPort.DsrHolding ? ConnectionStatus.ACTIVO : ConnectionStatus.INACTIVO;

            isConnectionOpen = returnValue;

            return returnValue;
        }

        /// <summary>
        /// Cierre del puerto serial
        /// </summary>
        public override void Close() {
            try {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.INFO, String.Format(tryAction, "Puerto", COMPort, "cerrar"));

                if (SerialPort != null && SerialPort.IsOpen)
                    SerialPort.Close();

                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(succes, "Puerto", COMPort, "cerró"));
            } catch (Exception) {
                ActivityLogCtrl.Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR, String.Format(error, "Puerto", COMPort, "cerrar"));
            }
        }

    }
}
