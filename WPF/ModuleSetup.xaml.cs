using APP;
using APP.Controller;
using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using static APP.Controller.AppModulesController;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Versioning;
using System.Collections;
using System.Windows.Controls;
using API;
using Microsoft.IdentityModel.Tokens;

namespace WPF
{
    /// <summary>
    /// Lógica de interacción para ModulesSetup.xaml
    /// </summary>
    public partial class ModulesSetup : Window
    {
        public ModulesSetup()
        {
            InitializeComponent();
            GetModuleList();
            ModulesDataGrid.Focus();
            this.DataContext = new ModulesDataContext();
        }

        /// <summary>
        /// Clase con contexto de la página
        /// </summary>
        public class ModulesDataContext
        {
            public Guid? ModuleId { get; set; }
            public string Name { get; set; }
            public ModuleTypeTranslated ModuleType { get; set; }
            public bool Active { get; set; }
            public bool RegisterLog { get; set; }
            public DateTime? AddedAt { get; set; }
            public ModulesDataContext()
            {

            }
            /// <summary>
            /// Recupera enumerador de ModuleType
            /// </summary>
            /// <returns>Enumerador ModuleType</returns>
            public APP.Module.ModuleType GetModuleType()
            {

                foreach (APP.Module.ModuleType type in Enum.GetValues(typeof(APP.Module.ModuleType)))
                {
                    if ((int)type == (int)ModuleType)
                        return type;
                }
                return default;
            }

            /// <summary>
            /// Recupera enumerador de ModuleTypeTranslated
            /// </summary>
            /// <returns>Enumerador ModuleTypeTranslated</returns>
            public static ModuleTypeTranslated GetTranslatedModuleType(APP.Module.ModuleType type)
            {
                foreach (ModuleTypeTranslated typeTranslated in Enum.GetValues(typeof(ModuleTypeTranslated)))
                {
                    if ((int)type == (int)typeTranslated)
                        return typeTranslated;
                }
                throw new Exception($"No existe tipo{type}");
            }

            public Array types
            {
                get { return Enum.GetValues(typeof(ModuleTypeTranslated)); }
            }

            public enum ModuleTypeTranslated
            {
                BASCULA = 1,
                FICHERO = 2,
                IMPRESORA = 3,
                REPETIDOR = 4,
                FTP = 5
            }
        }

        /// <summary>
        /// Recupera listado de Modulos
        /// </summary>
        private void GetModuleList()
        {
            List<ModulesDataContext> data = new List<ModulesDataContext>();
            List<AppModules> listedData = new AppModulesController().List(int.MaxValue);
            foreach (AppModules listedObjectData in listedData)
            {
                ModulesDataContext modContext = new ModulesDataContext();
                modContext.ModuleId = listedObjectData.moduleId;
                modContext.Name = listedObjectData.name;
                if (listedObjectData.active == true)
                    modContext.Active = true;
                if (listedObjectData.registerLog == true)
                    modContext.RegisterLog = true;
                modContext.ModuleType = ModulesDataContext.GetTranslatedModuleType(listedObjectData.type.Value);
                modContext.AddedAt = listedObjectData.addedAt;
                data.Add(modContext);
            }
            ModulesDataGrid.ItemsSource = data;
        }

        private void ModulesDataGrid_AddingNewItem(object sender, System.Windows.Controls.AddingNewItemEventArgs e)
        {
            e.NewItem = new ModulesDataContext()
            {
                ModuleId = Guid.NewGuid(),
                ModuleType = ModulesDataContext.ModuleTypeTranslated.FICHERO,
                AddedAt = DateTime.UtcNow
            };
        }
        private bool SaveModulesInfo() {

            var itemsSource = ModulesDataGrid.ItemsSource as IEnumerable;
            foreach (var item in itemsSource)
            {
                var row = ModulesDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                ModulesDataContext modContext = (ModulesDataContext)row.DataContext;
                AppModulesController modCtrl = new AppModulesController();
                AppModules mod = modCtrl.Get((Guid)modContext.ModuleId);
                if (mod != null)
                {
                    mod.active = modContext.Active;
                    mod.registerLog = modContext.RegisterLog;
                    mod.name = modContext.Name;
                    if (mod.type != modContext.GetModuleType())
                    {
                        MessageBox.Show($"No se puede cambiar de {modContext.ModuleType} a {ModulesDataContext.GetTranslatedModuleType(mod.type.Value)} en modulo {modContext.Name}", "Aviso",
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    modCtrl.Save(mod);
                }
                else
                {
                    modCtrl.Post(modContext.ModuleId.Value, modContext.GetModuleType(), modContext.Active,
                                    modContext.RegisterLog, modContext.Name);
                }
            }
            return true;
        }       

        [SupportedOSPlatform("windows")]
        private void Window_Closed(object sender, EventArgs e)
        {
            if (!SaveModulesInfo())
            {
                Application.Current.MainWindow = new ModulesSetup();
                Application.Current.MainWindow.Show();
            }
            else if (!App.GetConfiguredModules())
            {
                if (KestrelWebApp.StaticHost != null)
                    KestrelWebApp.Down();
                Application.Current.Shutdown();
            }            
            else
            {
                if (String.IsNullOrEmpty(App.GetRunCheckError()))
                {
                    Application.Current.MainWindow = new MainWindow();
                    Application.Current.MainWindow.Show();
                }
                else
                {
                    MessageBox.Show(App.GetRunCheckError(), "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.MainWindow = new ModulesSetup();
                    Application.Current.MainWindow.Show();
                }

            }

        }

        /// <summary>
        /// Recupera descripcion del enumerador
        /// </summary>
        /// <param name="value">Valor de enumerador</param>
        /// <returns>String con descripcion del enumerador</returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}
