
namespace ScaleModule
{
    public enum ScaleConfigId
    {
        /// <summary>
        /// Config de BALANZA
        /// </summary>
        SCALE_MODULE_COM_PORT = 2000,
        SCALE_MODULE_BAUDRATE = 2001,
        SCALE_MODULE_DATABITS = 2002,
        SCALE_MODULE_PARITY = 2003,
        SCALE_MODULE_STOPBITS = 2004,
        SCALE_MODULE_FLOWCONTROL = 2005,
        SCALE_MODULE_REQUEST_COMMAND = 2006,
        SCALE_MODULE_LIMIT_TO_TAKE = 2007,
        SCALE_MODULE_LOG_REGISTER_COM_STATUS = 2008,
        SCALE_MODULE_TCPIP_IP = 2009,
        SCALE_MODULE_TCPIP_PORT = 2010,
        SCALE_MODULE_COMUNICATION_TYPE = 2011,
    }

    public enum ScaleComunicationType {
        TCP_IP = 1,
        COM = 2
    }

}
