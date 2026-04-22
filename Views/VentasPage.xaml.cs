using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SistemaInventario.Models;
using SistemaInventario.Services;

namespace SistemaInventario.Views
{
    public partial class VentasPage : Page
    {
        private readonly ProductoService _productoService = new();
        private readonly VentaService _ventaService = new();

        private List<Producto> _productos = new();
        private List<VentaDetalle> _detalleVenta = new();

        public VentasPage()
        {
            InitializeComponent();
            CargarProductos();
            ActualizarGrid();
            LimpiarFormularioProducto();
            Loaded += VentasPage_Loaded;
        }

        private void VentasPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtEscaneo.Focus();
        }

        private void CargarProductos()
        {
            _productos = _productoService.ObtenerTodos()
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToList();

            CbProductos.ItemsSource = null;
            CbProductos.ItemsSource = _productos;
        }

        private void LimpiarFormularioProducto(bool limpiarBusqueda = false)
        {
            CbProductos.SelectedItem = null;
            TxtCantidad.Text = string.Empty;
            TxtPrecioUnitario.Text = string.Empty;
            TxtStockDisponible.Text = string.Empty;
            TxtSubtotal.Text = string.Empty;

            if (limpiarBusqueda)
                TxtEscaneo.Text = string.Empty;
        }

        private void ActualizarInfoProducto()
        {
            if (CbProductos.SelectedItem is not Producto producto)
            {
                TxtPrecioUnitario.Text = string.Empty;
                TxtStockDisponible.Text = string.Empty;
                TxtSubtotal.Text = string.Empty;
                return;
            }

            TxtPrecioUnitario.Text = producto.PrecioVenta.ToString("N2");
            TxtStockDisponible.Text = producto.Stock.ToString();

            if (int.TryParse(TxtCantidad.Text, out int cantidad) && cantidad > 0)
            {
                decimal subtotal = cantidad * producto.PrecioVenta;
                TxtSubtotal.Text = subtotal.ToString("N2");
            }
            else
            {
                TxtSubtotal.Text = "0.00";
            }
        }

        private void ActualizarGrid()
        {
            DgDetalleVenta.ItemsSource = null;
            DgDetalleVenta.ItemsSource = _detalleVenta;

            decimal total = _detalleVenta.Sum(x => x.Subtotal);
            int items = _detalleVenta.Sum(x => x.Cantidad);

            TxtTotalGeneral.Text = $"Bs {total:N2}";
            TxtTotalResumen.Text = $"Bs {total:N2}";
            TxtCantidadItems.Text = $"{items} ítems";
        }

        private void CbProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarInfoProducto();
        }

        private void TxtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarInfoProducto();
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (CbProductos.SelectedItem is not Producto producto)
            {
                MessageBox.Show("Primero busca o selecciona un producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int cantidad))
            {
                if (!int.TryParse(TxtCantidad.Text, out cantidad))
                {
                    MessageBox.Show("Cantidad inválida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                AgregarProductoAlDetalle(producto, cantidad);

                TxtEscaneo.Clear();
                TxtEscaneo.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnBuscarEscaneo_Click(object sender, RoutedEventArgs e)
        {
            ProcesarEscaneo();
        }

        private void TxtEscaneo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcesarEscaneo();
            }
        }

        private void ProcesarEscaneo()
        {
            string valorEscaneado = TxtEscaneo.Text.Trim();

            if (string.IsNullOrWhiteSpace(valorEscaneado))
            {
                TxtEscaneo.Focus();
                return;
            }

            if (!int.TryParse(valorEscaneado, out int productoId))
            {
                MessageBox.Show("El QR o valor ingresado debe contener el ID numérico del producto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtEscaneo.SelectAll();
                TxtEscaneo.Focus();
                return;
            }

            var producto = _productoService.ObtenerPorId(productoId);

            if (producto == null)
            {
                MessageBox.Show("No se encontró un producto con ese ID.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtEscaneo.SelectAll();
                TxtEscaneo.Focus();
                return;
            }

            if (producto.Stock <= 0)
            {
                MessageBox.Show("El producto no tiene stock disponible.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtEscaneo.SelectAll();
                TxtEscaneo.Focus();
                return;
            }

            CbProductos.SelectedItem = _productos.FirstOrDefault(p => p.Id == producto.Id);

            TxtCantidad.Text = string.Empty;
            TxtPrecioUnitario.Text = producto.PrecioVenta.ToString("N2");
            TxtStockDisponible.Text = producto.Stock.ToString();
            TxtSubtotal.Text = "0.00";

            TxtCantidad.Focus();
        }

        private void AgregarProductoAlDetalle(Producto producto, int cantidad)
        {
            int cantidadYaAgregada = _detalleVenta
                .Where(d => d.ProductoId == producto.Id)
                .Sum(d => d.Cantidad);

            int stockDisponibleReal = producto.Stock - cantidadYaAgregada;

            if (cantidad > stockDisponibleReal)
                throw new Exception("No hay suficiente stock disponible.");

            var detalleExistente = _detalleVenta.FirstOrDefault(d => d.ProductoId == producto.Id);

            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += cantidad;
                detalleExistente.Subtotal = detalleExistente.Cantidad * detalleExistente.PrecioUnitario;
            }
            else
            {
                var detalle = new VentaDetalle
                {
                    ProductoId = producto.Id,
                    Producto = producto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    Subtotal = cantidad * producto.PrecioVenta
                };

                _detalleVenta.Add(detalle);
            }

            ActualizarGrid();
            LimpiarFormularioProducto();
        }

        private void BtnQuitarSeleccionado_Click(object sender, RoutedEventArgs e)
        {
            if (DgDetalleVenta.SelectedItem is not VentaDetalle detalle)
            {
                MessageBox.Show("Selecciona un ítem del detalle.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _detalleVenta.Remove(detalle);
            ActualizarGrid();
        }

        private void BtnLimpiarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (!_detalleVenta.Any())
            {
                LimpiarFormularioProducto(true);
                TxtEscaneo.Focus();
                return;
            }

            var resultado = MessageBox.Show(
                "¿Deseas limpiar toda la venta actual?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            _detalleVenta.Clear();
            ActualizarGrid();
            LimpiarFormularioProducto(true);
            TxtEscaneo.Focus();
        }

        private void BtnGuardarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (!_detalleVenta.Any())
            {
                MessageBox.Show("No hay productos en la venta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var detallesParaGuardar = _detalleVenta.Select(d => new VentaDetalle
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList();

                _ventaService.RegistrarVenta(detallesParaGuardar);

                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                _detalleVenta.Clear();
                ActualizarGrid();
                CargarProductos();
                LimpiarFormularioProducto(true);
                TxtEscaneo.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}