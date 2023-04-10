using APP;
using APP.Controller;
using APP.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static APP.Controller.AppModulesController;

namespace ScaleModule {
    /// <summary>
    /// Lógica de interacción para ScalePage.xaml
    /// </summary>
    public partial class ScalePage : Page {
        public Guid moduleId { get; set; }
        public ScalePage(Guid moduleId) {
            InitializeComponent();
            LoadValuesFromSystem();

            ScaleDataContext scaleDataContext = new ScaleDataContext(moduleId);
            DataContext = scaleDataContext;
            scaleDataContext.PropertyChanged += StatusPropertyChanged;

            this.moduleId = moduleId;
        }

        private void StatusPropertyChanged(object sender, PropertyChangedEventArgs args) {
        }

        public static void c_ModuloEventReached(object sender, Dictionary<string, ModuleEvent> e) {
            AppModulesController moduleCtrl = new AppModulesController();
            List<AppModules> modulesList = moduleCtrl.List(int.MaxValue)
                                            .Where(a => a.type == ModuleType.WEIGHING_MACHINE)
                                            .ToList();

            foreach (AppModules mod in modulesList) {
                ModuleEvent moduleEvent = new ModuleEvent();
                moduleEvent.module = new Module();
                moduleEvent.module.id = (Guid)mod.moduleId;
                moduleEvent.module.name = mod.name;
                moduleEvent.module.type = (ModuleType)mod.type;
                moduleEvent.module.active = (bool)mod.active;
                moduleEvent.module.addedAt = mod.addedAt;
                e.Add(moduleEvent.module.id.ToString(), moduleEvent);
            }

        }
        /// <summary>
        /// Recupera valores del sistema
        /// </summary>
        private void LoadValuesFromSystem() {
            PortCOMInput.ItemsSource = SerialPort.GetPortNames();
            DataBitsInput.ItemsSource = new List<string> { "4", "5", "6", "7", "8" };
            ParityInput.ItemsSource = Enum.GetNames(typeof(Parity));
            StopBitInput.ItemsSource = Enum.GetNames(typeof(StopBits));
            FlowControlInput.ItemsSource = Enum.GetNames(typeof(Handshake));
        }

        private async void ReadWeight(object sender, MouseButtonEventArgs e) {
            Module module = (Module)StoreModules.GetModule(moduleId);
            string command = CommandText.Text;
            CommandResultText.Text = "Comunicando con dispositivo...";

            Task weightTest = Task.Run(() => {
                ConnectionStatus connectionStatus = module.ScaleManager.IsOpen();

                if (!connectionStatus.Equals(ConnectionStatus.ACTIVO))
                    connectionStatus = module.ScaleManager.OpenPort();

                if (!connectionStatus.Equals(ConnectionStatus.ACTIVO)) {
                    string message = String.Empty;

                    try {
                        message = new ActivityLogController().List(1, moduleId).First().message;
                    } catch {}

                    this.Dispatcher.Invoke(() => {
                        CommandResultText.Text = $"(!) {message}";
                    });

                    return;
                }

                string result;
                if (String.IsNullOrEmpty(command))
                    result = module.ScaleManager.GetMessage();
                else
                    result = module.ScaleManager.Send(command).GetMessage();

                this.Dispatcher.Invoke(() => {
                    if (String.IsNullOrEmpty(result)) {
                        CommandResultText.Text = $"(sin mensaje)";
                    } else {
                        CommandResultText.Text = $"{result}";
                    }
                });
            });

            await weightTest;
        }

    }
    /// <summary>
    /// Clase de contexto Bascula
    /// </summary>
    public class ScaleDataContext : INotifyPropertyChanged {
        public Guid moduleId {
            get;
            set;
        }
        public string moduleName {
            get {
                AppModules module = new AppModulesController().Get(moduleId);
                if (module == null)
                    return null;

                return module.name;
            }
            set {

            }

        }

        private bool _comunicationTypeCom;
        public bool comunicationTypeCom {
            get {
                _comunicationTypeCom = (ScaleComunicationType)Enum.Parse(typeof(ScaleComunicationType), configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_COMUNICATION_TYPE, moduleId)) == ScaleComunicationType.COM;
                return _comunicationTypeCom;
            }
            set {
                _comunicationTypeCom = value;

                if (_comunicationTypeCom) {
                    configCtrl.Save(ScaleConfigId.SCALE_MODULE_COMUNICATION_TYPE, ScaleComunicationType.COM.ToString(), moduleId);

                    UpdateComConfiguration();

                    this.comunicationTypeTcpIp = false;
                }

                OnPropertyChanged(nameof(comunicationTypeCom));
            }
        }

        private bool _comunicationTypeTcpIp;
        public bool comunicationTypeTcpIp {
            get {
                _comunicationTypeTcpIp = (ScaleComunicationType)Enum.Parse(typeof(ScaleComunicationType), configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_COMUNICATION_TYPE, moduleId)) == ScaleComunicationType.TCP_IP;
                return _comunicationTypeTcpIp;
            }
            set {
                _comunicationTypeTcpIp = value;

                if (_comunicationTypeTcpIp) {
                    configCtrl.Save(ScaleConfigId.SCALE_MODULE_COMUNICATION_TYPE, ScaleComunicationType.TCP_IP.ToString(), moduleId);

                    UpdateTcpIpConfiguration();

                    this.comunicationTypeCom = false;
                }

                OnPropertyChanged(nameof(comunicationTypeTcpIp));
            }
        }
        public string comPort {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_COM_PORT, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_COM_PORT, value, moduleId);
                UpdateComConfiguration();
            }
        }
        public string baudRate {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_BAUDRATE, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_BAUDRATE, value, moduleId);
                UpdateComConfiguration();
            }
        }
        public string dataBits {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_DATABITS, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_DATABITS, value.ToString(), moduleId);
                UpdateComConfiguration();
            }
        }
        public string parityValue {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_PARITY, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_PARITY, value.ToString(), moduleId);
                UpdateComConfiguration();
            }
        }
        public string stopBitValue {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_STOPBITS, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_STOPBITS, value.ToString(), moduleId);
                UpdateComConfiguration();
            }
        }
        public string tcpIpIp {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_TCPIP_IP, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_TCPIP_IP, value, moduleId);
                UpdateTcpIpConfiguration();
            }
        }
        public string tcpIpPort {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_TCPIP_PORT, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_TCPIP_PORT, value, moduleId);
                UpdateTcpIpConfiguration();
            }
        }
        public string flowControl {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_FLOWCONTROL, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_FLOWCONTROL, value.ToString(), moduleId);
                UpdateComConfiguration();
            }
        }
        public string requestCommand {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_REQUEST_COMMAND, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_REQUEST_COMMAND, value, moduleId);
            }
        }
        public string limitToTake {
            get {
                return configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_LIMIT_TO_TAKE, moduleId);
            }
            set {
                configCtrl.Save(ScaleConfigId.SCALE_MODULE_LIMIT_TO_TAKE, value, moduleId);
            }
        }

        private bool _RegisterLogCOMStatus;
        public bool RegisterLogCOMStatus {
            get {
                _RegisterLogCOMStatus = configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_LOG_REGISTER_COM_STATUS, moduleId) == "1";
                return _RegisterLogCOMStatus;
            }
            set {
                _RegisterLogCOMStatus = value;
                ((Module)StoreModules.GetModule(moduleId)).RegisterCOMStatusLog = _RegisterLogCOMStatus;
                if (_RegisterLogCOMStatus) {
                    configCtrl.Save(ScaleConfigId.SCALE_MODULE_LOG_REGISTER_COM_STATUS, "1", moduleId);
                } else {
                    configCtrl.Save(ScaleConfigId.SCALE_MODULE_LOG_REGISTER_COM_STATUS, "0", moduleId);
                }
            }
        }

        private bool _weighButtonEnabled;
        public bool weighButtonEnabled {
            get {
                return _weighButtonEnabled;
            }
            set {
                _weighButtonEnabled = value;

                OnPropertyChanged(nameof(weighButtonEnabled));
            }
        }

        private ConnectionStatus? _status;
        public ConnectionStatus? status {
            get {
                return _status;
            }
            set {
                _status = value;

                OnPropertyChanged(nameof(status));

                if (_status != null) {
                    statusText = _status.ToString();
                } else
                    statusText = ConnectionStatus.INACTIVO.ToString();

            }
        }
        private string _statusText;
        public string statusText {
            get {
                return _statusText;
            }
            set {
                _statusText = value;

                OnPropertyChanged(nameof(statusText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void IsConnectionOpenPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == nameof(module.ScaleManager.isConnectionOpen))
                status = module.ScaleManager != null ? module.ScaleManager.isConnectionOpen : ConnectionStatus.INACTIVO;
        }

        private void ScaleManagerPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == nameof(module.ScaleManager))
                weighButtonEnabled = module.ScaleManager != null;
        }

        private ConfigController configCtrl;

        private Module module;
        public ScaleDataContext(Guid moduleId) {
            configCtrl = new ConfigController();
            this.moduleId = moduleId;
            module = (Module)StoreModules.GetModule(moduleId);

            module.PropertyChanged += ScaleManagerPropertyChanged;

            // Configuración por defecto para los radio button del tipo de comunicación
            if (String.IsNullOrEmpty(configCtrl.GetValue(ScaleConfigId.SCALE_MODULE_COMUNICATION_TYPE, moduleId)))
                this.comunicationTypeCom = true;

            if (module.ScaleManager != null) {
                module.ScaleManager.PropertyChanged += IsConnectionOpenPropertyChanged;
                this.status = module.ScaleManager.IsOpen();
            }

            weighButtonEnabled = module.ScaleManager != null;
        }

        /// <summary>
        /// Actualizar configuracion y abre puerto
        /// </summary>
        public void UpdateComConfiguration() {
            Module module = (Module)StoreModules.GetModule(moduleId);

            if (module.ScaleManager != null) {
                module.ScaleManager.Close();
                module.ScaleManager = null;
            }

            if (module.InitCOMPort()) {
                module.ScaleManager.PropertyChanged += IsConnectionOpenPropertyChanged;
                this.status = module.StartPort();
            } else
                this.status = ConnectionStatus.INACTIVO;
        }

        /// <summary>
        /// Actualizar configuracion y abre la comunicación
        /// </summary>
        public void UpdateTcpIpConfiguration() {
            Module module = (Module)StoreModules.GetModule(moduleId);

            if (module.ScaleManager != null) {
                module.ScaleManager.Close();
                module.ScaleManager = null;
            }

            if (module.InitTcpIpPort()) {
                module.ScaleManager.PropertyChanged += IsConnectionOpenPropertyChanged;
                this.status = module.StartPort();
            } else
                this.status = ConnectionStatus.INACTIVO;
        }
    }

    /// <summary>
    /// Establece los distintos estados de la conexión con la báscula
    /// </summary>
    public enum ConnectionStatus {
        ACTIVO,
        INACTIVO,
        CONECTANDO
    }
}