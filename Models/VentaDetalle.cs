using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SistemaInventario.Models
{
    public class VentaDetalle
    {
        public int Id { get; set; }

        public int VentaId { get; set; }

        public Venta? Venta { get; set; }

        public int ProductoId { get; set; }

        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}