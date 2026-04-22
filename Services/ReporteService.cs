using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;

namespace SistemaInventario.Services
{
    public class ReporteService
    {
        public ReporteResumen ObtenerResumen(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = new AppDbContext();

            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1);

            var ventas = db.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .Where(v => v.Fecha >= inicio && v.Fecha < fin)
                .OrderByDescending(v => v.Fecha)
                .ToList();

            var productosStockBajo = db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Stock <= 5)
                .OrderBy(p => p.Stock)
                .ThenBy(p => p.Nombre)
                .ToList();

            var productosMasVendidos = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoId)
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key,
                    Nombre = g.First().Producto?.Nombre ?? "Sin nombre",
                    CantidadVendida = g.Sum(x => x.Cantidad),
                    TotalVendido = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .ThenByDescending(x => x.TotalVendido)
                .Take(10)
                .ToList();

            var ventasTabla = ventas.Select(v => new VentaTablaDto
            {
                Id = v.Id,
                Fecha = v.Fecha,
                CantidadItems = v.Detalles.Sum(d => d.Cantidad),
                Total = v.Total,
                ProductosTexto = string.Join(", ",
                    v.Detalles
                     .Where(d => d.Producto != null)
                     .Select(d => d.Producto!.Nombre)
                     .Distinct())
            }).ToList();

            return new ReporteResumen
            {
                FechaInicio = inicio,
                FechaFin = fechaFin.Date,
                CantidadVentas = ventas.Count,
                TotalVendido = ventas.Sum(v => v.Total),
                Ventas = ventas,
                VentasTabla = ventasTabla,
                ProductosStockBajo = productosStockBajo,
                ProductosMasVendidos = productosMasVendidos
            };
        }
    }

    public class ReporteResumen
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalVendido { get; set; }

        public List<Venta> Ventas { get; set; } = new();
        public List<VentaTablaDto> VentasTabla { get; set; } = new();
        public List<Producto> ProductosStockBajo { get; set; } = new();
        public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
    }

    public class VentaTablaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int CantidadItems { get; set; }
        public decimal Total { get; set; }
        public string ProductosTexto { get; set; } = string.Empty;
    }

    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
    }
}