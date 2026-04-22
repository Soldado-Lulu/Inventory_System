using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;

namespace SistemaInventario.Services
{
    public class ProductoService
    {
        public List<Producto> ObtenerTodos()
        {
            using var db = new AppDbContext();

            return db.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        public Producto? ObtenerPorId(int id)
        {
            using var db = new AppDbContext();

            return db.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Id == id);
        }

        public void Crear(Producto producto, string nombreCategoria)
        {
            using var db = new AppDbContext();

            string categoriaNormalizada = nombreCategoria.Trim();

            var categoriaExistente = db.Categorias
                .FirstOrDefault(c => c.Nombre.ToLower() == categoriaNormalizada.ToLower());

            if (categoriaExistente == null)
            {
                categoriaExistente = new Categoria
                {
                    Nombre = categoriaNormalizada
                };

                db.Categorias.Add(categoriaExistente);
                db.SaveChanges();
            }

            producto.CategoriaId = categoriaExistente.Id;

            db.Productos.Add(producto);
            db.SaveChanges();
        }

        public void Actualizar(Producto producto, string nombreCategoria)
        {
            using var db = new AppDbContext();

            string categoriaNormalizada = nombreCategoria.Trim();

            var categoriaExistente = db.Categorias
                .FirstOrDefault(c => c.Nombre.ToLower() == categoriaNormalizada.ToLower());

            if (categoriaExistente == null)
            {
                categoriaExistente = new Categoria
                {
                    Nombre = categoriaNormalizada
                };

                db.Categorias.Add(categoriaExistente);
                db.SaveChanges();
            }

            var productoDb = db.Productos.FirstOrDefault(p => p.Id == producto.Id);

            if (productoDb == null)
                return;

            productoDb.Nombre = producto.Nombre;
            productoDb.PrecioCompra = producto.PrecioCompra;
            productoDb.PrecioVenta = producto.PrecioVenta;
            productoDb.Stock = producto.Stock;
            productoDb.Descripcion = producto.Descripcion;
            productoDb.CategoriaId = categoriaExistente.Id;

            db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var db = new AppDbContext();

            var producto = db.Productos.FirstOrDefault(p => p.Id == id);

            if (producto != null)
            {
                db.Productos.Remove(producto);
                db.SaveChanges();
            }
        }

        public List<Categoria> ObtenerCategorias()
        {
            using var db = new AppDbContext();

            return db.Categorias
                .OrderBy(c => c.Nombre)
                .ToList();
        }
        public void AjustarStock(int productoId, int cantidad)
        {
            using var db = new AppDbContext();

            var producto = db.Productos.FirstOrDefault(p => p.Id == productoId);

            if (producto == null)
                throw new Exception("Producto no encontrado.");

            int nuevoStock = producto.Stock + cantidad;

            if (nuevoStock < 0)
                throw new Exception("No se puede dejar el stock en negativo.");

            producto.Stock = nuevoStock;
            db.SaveChanges();
        }
    }
}