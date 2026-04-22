using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SistemaInventario.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public int Stock { get; set; }

        // NUEVO
        public string Descripcion { get; set; } = string.Empty;

        // Relación categoría
        public int CategoriaId { get; set; }

        public Categoria? Categoria { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string CodigoQr { get; set; } = string.Empty;
        public bool UsaQr { get; set; } = true;
    }
}