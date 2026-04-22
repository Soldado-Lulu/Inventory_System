using System.Configuration;
using System.Data;
using System.Windows;
using SistemaInventario.Data;

namespace SistemaInventario
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DbInitializer.Inicializar();
            base.OnStartup(e);
        }
    }
}