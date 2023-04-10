using APP.Controller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ScaleModule {
    public abstract class ScaleManager: INotifyPropertyChanged {
        public readonly static int BUFFER_SIZE = 100;

        private Guid _ModuleId;
        public Guid ModuleId { get => _ModuleId; set => _ModuleId = value; }

        private Encoding _Encoding;
        public Encoding Encoding { get => _Encoding; set => _Encoding = value; }

        // Lecturas
        private ConcurrentQueue<byte[]> _MessageQueue;
        public ConcurrentQueue<byte[]> MessageQueue { get => _MessageQueue; set => _MessageQueue = value; }

        private EventWaitHandle _MessageWaitHandle;
        public EventWaitHandle MessageWaitHandle { get => _MessageWaitHandle; set => _MessageWaitHandle = value; }

        private ConnectionStatus _isConnectionOpen;
        public ConnectionStatus isConnectionOpen {
            get { return _isConnectionOpen; }
            set {
                _isConnectionOpen = value;
                OnPropertyChanged(nameof(isConnectionOpen));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// Log        
        public readonly static ActivityLogController ActivityLogCtrl = new ActivityLogController();

        public readonly static string errorGenericIO = "Error de inicialización del {0} {1}: {2}";
        public readonly static string tryAction = "Intento de {2} {0} {1}";
        public readonly static string error = "Error al {2} {0} {1}";
        public readonly static string succes = "Se {2} correctamente {0} {1}";
        public readonly static string errorPerdidaConexion = "Se ha perdido la conexión en {0}";
        public abstract ScaleManager Wait(int miliseconds);
        public abstract ScaleManager Send(string message);
        public abstract ConnectionStatus OpenPort();
        public abstract string GetMessage();
        /// <summary>
        /// Comprueba si la conexión está abierta
        /// </summary>
        /// <returns></returns>
        public abstract ConnectionStatus IsOpen();
        /// <summary>
        /// Cierra la conexión
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Obtiene el contenido escrito en el puerto serial
        /// </summary        
        /// <returns>String con mensaje</returns>  
        protected string GetMessageFromMessageQueue() {
            string returnValue = String.Empty;

            IEnumerable<byte[]> bytes = GetBytesFromMessageQueue(GetLimitToTake());

            byte[] fullData = Array.Empty<byte>();

            foreach (byte[] byteArr in bytes) {
                fullData = fullData.Concat(byteArr).ToArray();
            }

            returnValue = Encoding.GetString(fullData);

            return returnValue;
        }

        /// <summary>
        /// Obtiene el contenido escrito en el puerto
        /// </summary>
        /// <param name="limit">Cantidad de escrituras del puerto recuperar</param>
        /// <returns>Lista con bytes del mensaje</returns>
        private IEnumerable<byte[]> GetBytesFromMessageQueue(int limit) {
            IEnumerable<byte[]> returnValue = new List<byte[]>();

            if (MessageQueue != null) {
                if (MessageQueue.Count < limit)
                    returnValue = MessageQueue.TakeLast(MessageQueue.Count);
                else
                    returnValue = MessageQueue.TakeLast(limit);
            }

            return returnValue;
        }

        /// <summary>
        /// Lee el valor establecido en límite de lecturas
        /// </summary>
        /// <returns></returns>
        public int GetLimitToTake() {
            string limitToTakeString = new ConfigController().GetValue(ScaleConfigId.SCALE_MODULE_LIMIT_TO_TAKE, ModuleId);
            return String.IsNullOrEmpty(limitToTakeString) ? 1 : Convert.ToInt32(limitToTakeString);
        }

        /// <summary>
        /// Lee el valor del comando para recuperar el peso
        /// </summary>
        /// <returns></returns>
        public string GetWeighCommand() {
            return new ConfigController().GetValue(ScaleConfigId.SCALE_MODULE_REQUEST_COMMAND, ModuleId);
        }

        /// <summary>
        /// Si el tamaño de la cola es mayor que el tamaño de cola máximo elimina el primer mensaje;
        /// </summary>
        /// <returns>True, si se ha podido quitar el elemento de la cola</returns>
        public bool MessageQueueSizeControl() {
            bool returnValue = false;

            string limitToTakeString = new ConfigController().GetValue(ScaleConfigId.SCALE_MODULE_LIMIT_TO_TAKE, ModuleId);
            int queueMaxSize = 2 *(String.IsNullOrEmpty(limitToTakeString) ? 1 : Convert.ToInt32(limitToTakeString));

            if (MessageQueue != null && MessageQueue.Count > queueMaxSize)
                returnValue = MessageQueue.TryDequeue(out var queue);

            return returnValue;
        }

        /// <summary>
        /// Formatea el mensaje, interpretando los caracteres \n o \r\n
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string FormatMessage(string message) {
            string returnValue = message;

            if (returnValue != null)
                returnValue = Regex.Replace(returnValue, @"(\\n)|(\\r\\n)", System.Environment.NewLine);

            return returnValue;
        }

    }
}
