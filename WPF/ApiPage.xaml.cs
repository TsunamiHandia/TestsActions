using APP.Controller;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static APP.Controller.ApiActivityLogController;

namespace WPF
{
    /// <summary>
    /// Lógica de interacción para PaginaAPI.xaml
    /// </summary>
    public partial class ApiPage : Page
    {
        public ApiPage()
        {
            InitializeComponent();
            ActivityApiFilterInput.Text = "100";
            getListActivityLog(ActivityApiFilterInput.Text);
            if (RegistroActividadApiDataGrid.Items.Count > 0)
            {
                object item = RegistroActividadApiDataGrid.Items[0];
                RegistroActividadApiDataGrid.SelectedItem = item;
                setDetails((ApiDataContext)item);
            }
            RegistroActividadApiDataGrid.Focus();
        }

        private void getListActivityLog(string rowsTextBox)
        {
            int rows = 0;
            if (!int.TryParse(rowsTextBox, out rows))
            {
                RegistroActividadApiDataGrid.ItemsSource = null;
                return;
            }
            if (rows == 0 || rows < 0)
            {
                RegistroActividadApiDataGrid.ItemsSource = null;
                return;
            }

            List<ApiDataContext> data = new List<ApiDataContext>();
            List<ApiActivityLog> listedData = new ApiActivityLogController().List(rows);
            foreach (ApiActivityLog listedObjectData in listedData)
            {
                data.Add(new ApiDataContext()
                        {
                            id = listedObjectData.id,
                            target = listedObjectData.target,
                            status = listedObjectData.status,
                            requestType = listedObjectData.requestType,
                            resource = listedObjectData.resource,
                            reqHeaders = listedObjectData.reqHeaders,
                            reqBody = listedObjectData.reqBody,
                            respHeaders = listedObjectData.respHeaders,
                            respBody = listedObjectData.respBody,
                            addedAt = listedObjectData.addedAt,
                        }
                    );
            }
            RegistroActividadApiDataGrid.ItemsSource = data;
        }

        private void ActividadFiltroFilasInput_LostFocus(object sender, RoutedEventArgs e)
        {
            getListActivityLog(((TextBox)sender).Text);
        }

        private void RegistroActividadApiDataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender,
            e.OriginalSource as DependencyObject) as DataGridRow;

            if (row == null) return;
            
            setDetails((ApiDataContext)row.DataContext);
        }

        private void setDetails(ApiDataContext req)
        {
            InputRequestHeaders.Text = req.reqHeaders;
            InputRequestBody.Text = req.reqBody;
            InputResponseHeaders.Text = req.respHeaders;
            InputResponseBody.Text = req.respBody;
            InputResource.Text = req.resource;
            if (req.isSucessStatus)
            {
                                
                ImageRequest.Source = new BitmapImage(new Uri(@"/Resource/Images/ok.png", UriKind.Relative));
            }
            else
            {
                ImageRequest.Source = new BitmapImage(new Uri(@"/Resource/Images/ko.png", UriKind.Relative));
            }
            
        }

        public class ApiDataContext
        {
            public long? id { get; set; }
            public ApiActivityLog.Target? target { get; set; }
            public int? status { get; set; }
            public ApiActivityLog.RequestType? requestType { get; set; }
            public string resource { get; set; }
            public string reqHeaders { get; set; }
            public string reqBody { get; set; }
            public string respHeaders { get; set; }
            public string respBody { get; set; }
            public DateTime? addedAt { get; set; }

            public bool isSucessStatus
            {
                get { return ((int)status >= 200) && ((int)status <= 299); }
            }
        }

    }
}
