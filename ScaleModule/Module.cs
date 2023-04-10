using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using APP;
using APP.Controller;
using APP.Module;

namespace ScaleModule
{
    public class Module : APP.Module.Module, INotifyPropertyChanged {

        private ConfigController configCtrl = new ConfigController();
        private string ComPort;
        string BaudRate;
        string DataBits;
        string Parity;
        string StopBits;
        string FlowControl;
        string TcpIpIp;
        string TcpIpPort;
        string ComunicationType;
        public string RequestCommand;
        public bool RegisterCOMStatusLog;

        private ScaleManager _ScaleManager;

        public ScaleManager ScaleManager { 
            get => _ScaleManager; 
            set { 
                _ScaleManager = value;
                OnPropertyChanged("ScaleManager");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Module()
        {
            this.minConfigId = 2000;
            this.maxConfigId = 2999;
            this.name = "Scale Module";
            this.type = ModuleType.WEIGHING_MACHINE;

        }

        public override ButtonDecor getDecorButton()
        {
            ButtonDecor buttonDecor = new ButtonDecor();
            buttonDecor.Icon = "\uE967";
            return buttonDecor;
        }

        public override void initDB(SQL sql, bool recreate, bool seed)
        {

        }

        public override void up()
        {
            ComunicationType = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_COMUNICATION_TYPE, id);

            if (String.IsNullOrEmpty(ComunicationType))
                return;

            ScaleComunicationType scaleComunicationType = (ScaleComunicationType)Enum.Parse(typeof(ScaleComunicationType), ComunicationType);

            switch (scaleComunicationType) {
                case ScaleComunicationType.COM:
                    if (InitCOMPort())
                        StartPort();
                    break;
                case ScaleComunicationType.TCP_IP:
                    if (InitTcpIpPort())
                        StartPort();
                    break;
                default: break;
            }
        }

        public override bool HealthCheck()
        {
            try
            {
                if (!isActiveRemote())
                {
                    new ActivityLogController()
                        .Post(id, ActivityLogController.ActivityLog.Status.ERROR, "Modulo no está configurado en BC");

                    return false;
                }
            }
            catch (Exception ex)
            {
                new ActivityLogController().Post(id, ActivityLogController.ActivityLog.Status.ERROR, ex.Message);
                return false;
            }

            ActivityLogController.ActivityLog activityLog = new ActivityLogController().List(1, id)
                                                    .Find(a => a.status == ActivityLogController.ActivityLog.Status.ERROR);
            if (activityLog != null)
                return false;

            if (ScaleManager == null || (ScaleManager != null && !ScaleManager.isConnectionOpen.Equals(ConnectionStatus.ACTIVO)))
                return false;

            return true;
        }

        /// <summary>
        /// Inicializa puerto COM
        /// </summary>
        /// <returns>True en caso de haber podido inicializar el puerto</returns>
        public bool InitCOMPort() {            

            bool returnValue = false;

            ComPort = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_COM_PORT, id);
            BaudRate = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_BAUDRATE, id);
            DataBits = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_DATABITS, id);
            Parity = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_PARITY, id);
            StopBits = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_STOPBITS, id);
            FlowControl = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_FLOWCONTROL, id);
            RequestCommand = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_REQUEST_COMMAND, id);

            try {
                // Inicializar gestor de bascula
                ScaleManager = new ScaleComManager(ComPort, BaudRate, DataBits, Parity, StopBits, FlowControl);
                ScaleManager.ModuleId = id;
                returnValue = true;
            } catch (ArgumentNullException ex) {
                ScaleManager = null;
            }

            return returnValue;
        }

        /// <summary>
        /// Inicializa puerto TCP/IP
        /// </summary>
        /// <returns>True en caso de haber podido inicializar el puerto</returns>
        public bool InitTcpIpPort() {

            bool returnValue = false;

            TcpIpIp = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_TCPIP_IP, id);
            TcpIpPort = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_TCPIP_PORT, id);
            RequestCommand = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_REQUEST_COMMAND, id);

            try {
                /// Inicializar gestor de bascula
                ScaleManager = new ScaleTcpIpManager(TcpIpIp, TcpIpPort);
                ScaleManager.ModuleId = id;
                returnValue = true;
            } catch (ArgumentNullException ex) {
                ScaleManager = null;
            }

            return returnValue;
        }

        /// <summary>
        /// Arranca la escucha del puerto
        /// </summary>
        /// <returns></returns>
        public ConnectionStatus StartPort() {
            return ScaleManager.OpenPort();
        }
    }
}
