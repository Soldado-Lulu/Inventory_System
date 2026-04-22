using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using SistemaInventario.Services;
using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class ReportesPage : Page
    {
        private readonly ReporteService _reporteService = new();
        private bool _mostrarGraficas = false;

        public ReportesPage()
        {
            InitializeComponent();
            InicializarFechas();
            PanelGraficas.Visibility = Visibility.Collapsed;
            CargarReporte();
        }

        private void InicializarFechas()
        {
            DpFechaInicio.SelectedDate = DateTime.Today;
            DpFechaFin.SelectedDate = DateTime.Today;
        }

        private void CargarReporte()
        {
            DateTime fechaInicio = DpFechaInicio.SelectedDate ?? DateTime.Today;
            DateTime fechaFin = DpFechaFin.SelectedDate ?? DateTime.Today;

            if (fechaInicio > fechaFin)
            {
                MessageBox.Show("La fecha inicio no puede ser mayor que la fecha final.");
                return;
            }

            var resumen = _reporteService.ObtenerResumen(fechaInicio, fechaFin);

            TxtTotalVendido.Text = $"Bs {resumen.TotalVendido:N2}";
            TxtCantidadVentas.Text = resumen.CantidadVentas.ToString();
            TxtStockBajoResumen.Text = resumen.ProductosStockBajo.Count.ToString();

            DgVentas.ItemsSource = null;
            DgVentas.ItemsSource = resumen.VentasTabla;

            DgStockBajo.ItemsSource = null;
            DgStockBajo.ItemsSource = resumen.ProductosStockBajo;

            ConstruirGraficoVentasPastel(resumen);
            ConstruirGraficoProductosBarras(resumen);
        }

        private void ConstruirGraficoVentasPastel(ReporteResumen resumen)
        {
            var model = new PlotModel
            {
                Title = "Ventas por producto"
            };

            model.Legends.Add(new Legend
            {
                LegendPosition = LegendPosition.RightTop
            });

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                InsideLabelPosition = 0.7,
                AngleSpan = 360,
                StartAngle = 0
            };

            foreach (var item in resumen.ProductosMasVendidos)
            {
                if (item.CantidadVendida > 0)
                {
                    pieSeries.Slices.Add(new PieSlice(item.Nombre, item.CantidadVendida));
                }
            }

            model.Series.Add(pieSeries);
            PlotVentasPastel.Model = model;
        }

        private void ConstruirGraficoProductosBarras(ReporteResumen resumen)
        {
            var model = new PlotModel
            {
                Title = "Productos más vendidos"
            };

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                Title = "Cantidad"
            };

            var barSeries = new BarSeries
            {
                StrokeThickness = 1
            };

            var top = resumen.ProductosMasVendidos
                .Take(8)
                .OrderBy(x => x.CantidadVendida)
                .ToList();

            foreach (var item in top)
            {
                categoryAxis.Labels.Add(item.Nombre);
                barSeries.Items.Add(new BarItem { Value = item.CantidadVendida });
            }

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);
            model.Series.Add(barSeries);

            PlotProductosBarras.Model = model;
        }
        private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            CargarReporte();
        }

        private void BtnHoy_Click(object sender, RoutedEventArgs e)
        {
            DpFechaInicio.SelectedDate = DateTime.Today;
            DpFechaFin.SelectedDate = DateTime.Today;
            CargarReporte();
        }

        private void BtnMostrarGraficas_Click(object sender, RoutedEventArgs e)
        {
            _mostrarGraficas = !_mostrarGraficas;

            PanelGraficas.Visibility = _mostrarGraficas
                ? Visibility.Visible
                : Visibility.Collapsed;

            BtnMostrarGraficas.Content = _mostrarGraficas
                ? "Ocultar gráficas"
                : "Mostrar gráficas";
        }
    }
}