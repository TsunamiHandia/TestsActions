using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using APP;
using APP.Controller;
namespace WPF
{
    /// <summary>
    /// Lógica de interacción para PaginaRegistroActividad.xaml
    /// </summary>
    public partial class PaginaRegistroActividad : Page
    {
        public PaginaRegistroActividad()
        {
            InitializeComponent();
            ActividadFiltroFilasInput.Text = "100";
            getListActivityLog(ActividadFiltroFilasInput.Text);            
        }

        private void getListActivityLog(string rowsTextBox)
        {
            int rows = 0;
            if (!int.TryParse(rowsTextBox, out rows))
            {
                RegistroActividadDataGrid.ItemsSource = null;
                return;
            }
            if (rows == 0 || rows < 0)
            {
                RegistroActividadDataGrid.ItemsSource = null;
                return;
            }            

            List<ActivityLogContext> data = new List<ActivityLogContext>();
            List<ActivityLogController.ActivityLog> listedData = new ActivityLogController().List(rows);
            foreach (ActivityLogController.ActivityLog listedObjectData in listedData)
            {
                data.Add(new ActivityLogContext()
                        {
                            id = listedObjectData.id,
                            status = listedObjectData.status,
                            moduleId = listedObjectData.moduleId,
                            message = listedObjectData.message,
                            stackTrace = listedObjectData.stackTrace,
                            addedAt = listedObjectData.addedAt
                        }
                    );
            }
            RegistroActividadDataGrid.ItemsSource = data;
        }

        private void ActividadFiltroFilasInput_LostFocus(object sender, RoutedEventArgs e)
        {            
            getListActivityLog(((TextBox)sender).Text);
        }

        public class ActivityLogContext
        {
            public int? id { get; set; }

            public ActivityLogController.ActivityLog.Status? status { get; set; }

            public Guid? moduleId { get; set; }

            public string message { get; set; }

            public string stackTrace { get; set; }

            public DateTime? addedAt { get; set; }

            public string moduleName
            {
                get
                {
                    AppModulesController.AppModules module = new AppModulesController().Get(moduleId.Value);
                    if (module == null)
                    {
                        return null;
                    }
                    return module.name;
                }
            }
        }

    }
}
