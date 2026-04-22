using System;
using System.Collections.Generic;
using System.Linq;
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

        // Buscar producto por código interno/manual
        public Producto? ObtenerPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            string codigoNormalizado = codigo.Trim();

            using var db = new AppDbContext();

            return db.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Codigo.ToLower() == codigoNormalizado.ToLower());
        }

        // Buscar producto por código QR escaneado
        public Producto? ObtenerPorCodigoQr(string codigoQr)
        {
            if (string.IsNullOrWhiteSpace(codigoQr))
                return null;

            string codigoQrNormalizado = codigoQr.Trim();

            using var db = new AppDbContext();

            return db.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.CodigoQr.ToLower() == codigoQrNormalizado.ToLower());
        }

        // Método flexible para caja:
        // primero intenta por CódigoQr y luego por Código
        public Producto? ObtenerPorCodigoEscaneado(string valorEscaneado)
        {
            if (string.IsNullOrWhiteSpace(valorEscaneado))
                return null;

            string valor = valorEscaneado.Trim();

            using var db = new AppDbContext();

            return db.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p =>
                    p.CodigoQr.ToLower() == valor.ToLower() ||
                    p.Codigo.ToLower() == valor.ToLower());
        }

        public void Crear(Producto producto, string nombreCategoria)
        {
            using var db = new AppDbContext();

            if (producto == null)
                throw new Exception("El producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                throw new Exception("El nombre del producto es obligatorio.");

            string categoriaNormalizada = nombreCategoria?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(categoriaNormalizada))
                throw new Exception("La categoría es obligatoria.");

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

            // Si no mandan código, se genera uno automático simple
            if (string.IsNullOrWhiteSpace(producto.Codigo))
            {
                producto.Codigo = GenerarCodigoProducto(db);
            }
            else
            {
                producto.Codigo = producto.Codigo.Trim();
            }

            // Si no mandan código QR, usamos el mismo código del producto
            if (string.IsNullOrWhiteSpace(producto.CodigoQr))
            {
                producto.CodigoQr = producto.Codigo;
            }
            else
            {
                producto.CodigoQr = producto.CodigoQr.Trim();
            }

            // Validar duplicados
            bool codigoExiste = db.Productos.Any(p => p.Codigo.ToLower() == producto.Codigo.ToLower());
            if (codigoExiste)
                throw new Exception("Ya existe un producto con ese código.");

            bool codigoQrExiste = db.Productos.Any(p => p.CodigoQr.ToLower() == producto.CodigoQr.ToLower());
            if (codigoQrExiste)
                throw new Exception("Ya existe un producto con ese código QR.");

            producto.Nombre = producto.Nombre.Trim();
            producto.Descripcion = producto.Descripcion?.Trim() ?? string.Empty;
            producto.CategoriaId = categoriaExistente.Id;

            db.Productos.Add(producto);
            db.SaveChanges();
        }

        public void Actualizar(Producto producto, string nombreCategoria)
        {
            using var db = new AppDbContext();

            if (producto == null)
                throw new Exception("El producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                throw new Exception("El nombre del producto es obligatorio.");

            string categoriaNormalizada = nombreCategoria?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(categoriaNormalizada))
                throw new Exception("La categoría es obligatoria.");

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
                throw new Exception("Producto no encontrado.");

            string codigoNormalizado = string.IsNullOrWhiteSpace(producto.Codigo)
                ? productoDb.Codigo
                : producto.Codigo.Trim();

            string codigoQrNormalizado = string.IsNullOrWhiteSpace(producto.CodigoQr)
                ? codigoNormalizado
                : producto.CodigoQr.Trim();

            bool codigoExiste = db.Productos.Any(p =>
                p.Id != producto.Id &&
                p.Codigo.ToLower() == codigoNormalizado.ToLower());

            if (codigoExiste)
                throw new Exception("Ya existe otro producto con ese código.");

            bool codigoQrExiste = db.Productos.Any(p =>
                p.Id != producto.Id &&
                p.CodigoQr.ToLower() == codigoQrNormalizado.ToLower());

            if (codigoQrExiste)
                throw new Exception("Ya existe otro producto con ese código QR.");

            productoDb.Nombre = producto.Nombre.Trim();
            productoDb.Codigo = codigoNormalizado;
            productoDb.CodigoQr = codigoQrNormalizado;
            productoDb.PrecioCompra = producto.PrecioCompra;
            productoDb.PrecioVenta = producto.PrecioVenta;
            productoDb.Stock = producto.Stock;
            productoDb.Descripcion = producto.Descripcion?.Trim() ?? string.Empty;
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

        // Genera un código simple consecutivo tipo PROD-0001
        private string GenerarCodigoProducto(AppDbContext db)
        {
            int ultimoId = db.Productos
                .OrderByDescending(p => p.Id)
                .Select(p => p.Id)
                .FirstOrDefault();

            int siguiente = ultimoId + 1;

            return $"PROD-{siguiente:D4}";
        }
    }
}