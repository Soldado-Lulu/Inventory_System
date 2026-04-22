using Microsoft.EntityFrameworkCore;
using SistemaInventario.Models;
using System.IO;

namespace SistemaInventario.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventario.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.Nombre)
                .IsUnique();

            modelBuilder.Entity<Categoria>()
                .Property(c => c.Nombre)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .Property(p => p.Nombre)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .Property(p => p.Descripcion)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioCompra)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.Codigo)
                .IsUnique();
            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.CodigoQr)
                .IsUnique();

            modelBuilder.Entity<Producto>()
                .Property(p => p.Codigo)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .Property(p => p.CodigoQr)
                .IsRequired();
            modelBuilder.Entity<Producto>()
    .Property(p => p.UsaQr)
    .HasDefaultValue(true);
            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(vd => vd.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Producto)
                .WithMany()
                .HasForeignKey(vd => vd.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}