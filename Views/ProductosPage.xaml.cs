using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SistemaInventario.Models;
using SistemaInventario.Services;

namespace SistemaInventario.Views
{
    public partial class ProductosPage : Page
    {
        private readonly ProductoService _productoService = new();
        private Producto? _productoSeleccionado;
        private List<Producto> _productos = new();

        public ProductosPage()
        {
            InitializeComponent();
            CargarCategorias();
            CargarProductos();
        }

        private void CargarCategorias()
        {
            var categorias = _productoService.ObtenerCategorias();
            CbCategoria.ItemsSource = null;
            CbCategoria.ItemsSource = categorias;
        }

        private void CargarProductos()
        {
            _productos = _productoService.ObtenerTodos();
            DgProductos.ItemsSource = null;
            DgProductos.ItemsSource = _productos;
            ActualizarResumen();
            CargarCategorias();
        }

        private void ActualizarResumen()
        {
            TxtTotalProductos.Text = $"{_productos.Count} productos";
            TxtStockBajo.Text = $"{_productos.Count(p => p.Stock <= 5)} productos";
            TxtTotalCategorias.Text = $"{_productos.Select(p => p.CategoriaId).Distinct().Count()} categorías";
        }

        private void LimpiarFormulario()
        {
            TxtNombre.Text = string.Empty;
            TxtCodigo.Text = string.Empty;
            TxtCodigoQr.Text = string.Empty;
            ChkUsaQr.IsChecked = true;
            TxtPrecioCompra.Text = string.Empty;
            TxtPrecioVenta.Text = string.Empty;
            TxtStock.Text = string.Empty;
            TxtDescripcion.Text = string.Empty;
            TxtBuscar.Text = string.Empty;
            CbCategoria.Text = string.Empty;
            CbCategoria.SelectedItem = null;

            _productoSeleccionado = null;
            DgProductos.SelectedItem = null;
            DgProductos.ItemsSource = null;
            DgProductos.ItemsSource = _productos;
        }

        private bool ValidarFormulario(
            out decimal precioCompra,
            out decimal precioVenta,
            out int stock,
            out string nombreCategoria)
        {
            precioCompra = 0;
            precioVenta = 0;
            stock = 0;
            nombreCategoria = CbCategoria.Text.Trim();

            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nombreCategoria))
            {
                MessageBox.Show("La categoría es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(TxtPrecioCompra.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out precioCompra))
            {
                if (!decimal.TryParse(TxtPrecioCompra.Text, out precioCompra))
                {
                    MessageBox.Show("Precio de compra inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!decimal.TryParse(TxtPrecioVenta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out precioVenta))
            {
                if (!decimal.TryParse(TxtPrecioVenta.Text, out precioVenta))
                {
                    MessageBox.Show("Precio de venta inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!int.TryParse(TxtStock.Text, out stock))
            {
                MessageBox.Show("Stock inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (precioCompra < 0 || precioVenta < 0 || stock < 0)
            {
                MessageBox.Show("No se permiten valores negativos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario(out decimal precioCompra, out decimal precioVenta, out int stock, out string nombreCategoria))
                return;

            try
            {
                var producto = new Producto
                {
                    Nombre = TxtNombre.Text.Trim(),
                    Codigo = TxtCodigo.Text.Trim(),
                    CodigoQr = TxtCodigoQr.Text.Trim(),
                    UsaQr = ChkUsaQr.IsChecked ?? true,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    Stock = stock,
                    Descripcion = TxtDescripcion.Text.Trim()
                };

                _productoService.Crear(producto, nombreCategoria);
                CargarProductos();
                LimpiarFormulario();

                MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto para actualizar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidarFormulario(out decimal precioCompra, out decimal precioVenta, out int stock, out string nombreCategoria))
                return;

            try
            {
                _productoSeleccionado.Nombre = TxtNombre.Text.Trim();
                _productoSeleccionado.Codigo = TxtCodigo.Text.Trim();
                _productoSeleccionado.CodigoQr = TxtCodigoQr.Text.Trim();
                _productoSeleccionado.UsaQr = ChkUsaQr.IsChecked ?? true;
                _productoSeleccionado.PrecioCompra = precioCompra;
                _productoSeleccionado.PrecioVenta = precioVenta;
                _productoSeleccionado.Stock = stock;
                _productoSeleccionado.Descripcion = TxtDescripcion.Text.Trim();

                _productoService.Actualizar(_productoSeleccionado, nombreCategoria);
                CargarProductos();
                LimpiarFormulario();

                MessageBox.Show("Producto actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Deseas eliminar el producto '{_productoSeleccionado.Nombre}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            _productoService.Eliminar(_productoSeleccionado.Id);
            CargarProductos();
            LimpiarFormulario();

            MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void DgProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgProductos.SelectedItem is not Producto producto)
                return;

            _productoSeleccionado = producto;

            TxtNombre.Text = producto.Nombre;
            TxtCodigo.Text = producto.Codigo;
            TxtCodigoQr.Text = producto.CodigoQr;
            ChkUsaQr.IsChecked = producto.UsaQr;
            TxtPrecioCompra.Text = producto.PrecioCompra.ToString("0.##");
            TxtPrecioVenta.Text = producto.PrecioVenta.ToString("0.##");
            TxtStock.Text = producto.Stock.ToString();
            TxtDescripcion.Text = producto.Descripcion;
            CbCategoria.Text = producto.Categoria?.Nombre ?? string.Empty;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = TxtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(texto))
            {
                DgProductos.ItemsSource = null;
                DgProductos.ItemsSource = _productos;
                return;
            }

            var filtrados = _productos
                .Where(p =>
                    p.Nombre.ToLower().Contains(texto) ||
                    p.Codigo.ToLower().Contains(texto) ||
                    p.CodigoQr.ToLower().Contains(texto) ||
                    (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(texto)))
                .ToList();

            DgProductos.ItemsSource = null;
            DgProductos.ItemsSource = filtrados;
        }
    }
}