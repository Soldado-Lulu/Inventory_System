using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaInventario.Models;

namespace SistemaInventario.Data
{
    public static class DbInitializer
    {
        public static void Inicializar()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
        }
    }
}