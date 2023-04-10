using API;
using APP;
using System.Windows;
using System.Windows.Input;

namespace WPF
{
    ///Asigna comandos básicos al Icono de Noificacion
    public class NotifyIconViewModel
    {
        /// Muestra la aplicacion (solo si ya no esta activa) 
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow == null,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow = App.NavigateTo();
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        ///Cierre de la aplicacion
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { 
                    CommandAction = () => {
                        KestrelWebApp.Down();
                        Application.Current.Shutdown();
                    }
                };
            }
        }
    }
}
