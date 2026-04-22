using Microsoft.EntityFrameworkCore;
using SistemaInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaInventario.Data;
namespace SistemaInventario.Services
{
    public class VentaService
    {
        public void RegistrarVenta(List<VentaDetalle> detalles)
        {
            using var db = new AppDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                if (detalles == null || !detalles.Any())
                    throw new Exception("No hay detalles para registrar la venta.");

                decimal total = detalles.Sum(d => d.Subtotal);

                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    Total = total
                };

                db.Ventas.Add(venta);
                db.SaveChanges();

                foreach (var detalle in detalles)
                {
                    var producto = db.Productos.FirstOrDefault(p => p.Id == detalle.ProductoId);

                    if (producto == null)
                        throw new Exception("Producto no encontrado.");

                    if (producto.Stock < detalle.Cantidad)
                        throw new Exception($"Stock insuficiente para {producto.Nombre}.");

                    producto.Stock -= detalle.Cantidad;

                    var nuevoDetalle = new VentaDetalle
                    {
                        VentaId = venta.Id,
                        ProductoId = detalle.ProductoId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal
                    };

                    db.VentaDetalles.Add(nuevoDetalle);
                }

                db.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Venta> ObtenerVentas()
        {
            using var db = new AppDbContext();

            return db.Ventas
                .OrderByDescending(v => v.Fecha)
                .ToList();
        }
    }
}