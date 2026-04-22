using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
        }

        private void BtnVender_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new VentasPage());
        }

        private void BtnProductos_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProductosPage());
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new InventarioPage());
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ReportesPage());
        }
    }
}