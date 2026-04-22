using static System.Net.Mime.MediaTypeNames;
using System.Reflection.PortableExecutable;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows;
using System.Xml.Linq;
using SistemaInventario.Views;

namespace SistemaInventario
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new DashboardPage());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void Productos_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductosPage());
        }

        private void Ventas_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new VentasPage());
        }
        private void Inventario_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InventarioPage());
        }
        private void Reportes_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportesPage());
        }
       
        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}