using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF
{
    /// <summary>
    /// Lógica de interacción para Configuracion.xaml
    /// </summary>
    public partial class PaginaConfiguracion : Page
    {
        public PaginaConfiguracion()
        {
            InitializeComponent();
        }


        private void ConfigTenantInput_LostFocus(object sender, RoutedEventArgs e)
        {            
            TextBox input = (TextBox)sender;
            if (Utils.isValid(input.Text))
            {
                APP.Models.Configuracion conf = APP.ConfigsController.Get(1);
                string url = conf.TenantId;
                if (!url.Equals(input.Text))
                {
                    conf.TenantId = input.Text;
                    APP.ConfigsController.Set(1,conf);                   
                    //MessageBox.Show(String.Format("Se ha modificado el TennantId, Antes:{0} y Ahora:{1}", url, APP.ControllerConfiguracion.Get().TenantId));
                }                
                status.Source = Utils.GetImage("correcto.png");
            }
            else status.Source = Utils.GetImage("erroneo.png");
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            APP.Models.Configuracion conf = APP.ConfigsController.Get(1);
            ConfigTenantInput.Text = conf.TenantId;

            if (!Utils.isValid(conf.TenantId))  status.Source = Utils.GetImage("erroneo.png");
            else status.Source = Utils.GetImage("correcto.png");               
            
            if (conf.Entorno == 1)
            {
                ConfigEntornoCloud.IsChecked = true;
            }
            else { 
                ConfigEntornoOnPremise.IsChecked = true;
            }
       

        }

      

        private void ConfigEntornoCloud_Checked(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("OnCloud");
            APP.Models.Configuracion conf = APP.ConfigsController.Get(1);
            RadioButton oncloud = (RadioButton)sender;
            conf.Entorno = 1;
            APP.ConfigsController.Set(1,conf);

        }

        private void ConfigEntornoOnPremise_Checked(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("OnPrem");
            APP.Models.Configuracion conf = APP.ConfigsController.Get(1);
            RadioButton onprem = (RadioButton)sender;
            conf.Entorno = 2;
            APP.ConfigsController.Set(1,conf);
        }
    }
}
