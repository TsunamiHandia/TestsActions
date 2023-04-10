using APP;
using APP.Module;
using APP.Controller;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http;

namespace WPF
{
    /// <summary>
    /// Lógica de interacción para PaginaInicio.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();          
            MainPageDataContext context = new MainPageDataContext();
            context.Icons = GetActiveIcons();
            DataContext = context;            
        }

        /// <summary>
        /// Crea iconos en base a los modulos activos
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<IconStatus> GetActiveIcons()
        {
            ObservableCollection<IconStatus> icons = new ObservableCollection<IconStatus>();            
            foreach (KeyValuePair<string, ModuleEvent> moduleObj in StoreModules.modulesCollection)
            {
                Module module = moduleObj.Value.module;
                if (module.active)
                {     
                    icons.Add(new IconStatus()
                    {
                        id = module.id,
                        Name = module.name,
                        Icon = module.getDecorButton().Icon,
                        CurrentStatus = Status.LOADING
                    });                                 
                    
                }
            }
            return icons;
        }

        /// <summary>
        /// Asigna estado a los iconos
        /// </summary>
        /// <param name="sender">Pagina de la ventana</param>
        private async void CheckStatus(MainPage sender)
        {
            MainPageDataContext context = (MainPageDataContext)sender.DataContext;

            Task task = Task.Run(() =>
               {
                   /// Modules Status
                   foreach (var item in context.Icons)
                   {
                       Module module = StoreModules.GetModule(item.id);

                       item.CurrentStatus = Status.OK;
                       if (!module.HealthCheck())
                           item.CurrentStatus = Status.ERROR;                       

                   }
                   
                   this.Dispatcher.Invoke(() =>
                   {
                       this.DataContext = null;
                       this.DataContext = context;                       
                   });
               }
            );

            await task;            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {            
            CheckStatus((MainPage)sender);            
        }
    }


    public class MainPageDataContext
    {
        public ObservableCollection<IconStatus> Icons { get; set; }
        
    }

    public class IconStatus
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public Status CurrentStatus { get; set; }
                
    }

    public enum Status { ERROR = 0, OK = 1, LOADING = 2 }


}
