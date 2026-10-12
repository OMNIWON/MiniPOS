using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace POS.Data;

public class PosDbContext : DbContext
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<DetalleCotizacion> DetallesCotizacion => Set<DetalleCotizacion>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ConfiguracionNegocio> Configuraciones => Set<ConfiguracionNegocio>();

    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venta>()
    .Property(v => v.TasaIva).HasColumnType("decimal(5,2)");
        modelBuilder.Entity<ConfiguracionNegocio>()
            .Property(c => c.TasaIva).HasColumnType("decimal(5,2)");
        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioCompra).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioVenta).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Producto>()
            .Property(p => p.Existencia).HasColumnType("decimal(18,3)");
        modelBuilder.Entity<Producto>()
            .Property(p => p.StockMinimo).HasColumnType("decimal(18,3)");

        modelBuilder.Entity<Venta>()
            .Property(v => v.Subtotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Venta>()
            .Property(v => v.Impuestos).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Venta>()
            .Property(v => v.Total).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Venta>()
            .Property(v => v.DineroRecibido).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Venta>()
            .Property(v => v.Cambio).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<DetalleVenta>()
            .Property(d => d.Cantidad).HasColumnType("decimal(18,3)");
        modelBuilder.Entity<DetalleVenta>()
            .Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<DetalleVenta>()
            .Property(d => d.DescuentoPorcentaje).HasColumnType("decimal(5,2)");
        modelBuilder.Entity<DetalleVenta>()
            .Property(d => d.Subtotal).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Cotizacion>()
            .Property(c => c.Total).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<DetalleCotizacion>()
            .Property(d => d.Cantidad).HasColumnType("decimal(18,3)");
        modelBuilder.Entity<DetalleCotizacion>()
            .Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<DetalleCotizacion>()
            .Property(d => d.DescuentoPorcentaje).HasColumnType("decimal(5,2)");
        modelBuilder.Entity<DetalleCotizacion>()
            .Property(d => d.Subtotal).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Venta>()
            .HasMany(v => v.Detalles)
            .WithOne(d => d.Venta)
            .HasForeignKey(d => d.VentaId);

        modelBuilder.Entity<Producto>()
            .HasMany<DetalleVenta>()
            .WithOne(d => d.Producto)
            .HasForeignKey(d => d.ProductoId);

        modelBuilder.Entity<Categoria>()
            .HasMany(c => c.Productos)
            .WithOne(p => p.Categoria)
            .HasForeignKey(p => p.CategoriaId);

        modelBuilder.Entity<Cotizacion>()
            .HasMany(c => c.Detalles)
            .WithOne(d => d.Cotizacion)
            .HasForeignKey(d => d.CotizacionId);

        modelBuilder.Entity<Producto>()
            .HasMany<DetalleCotizacion>()
            .WithOne(d => d.Producto)
            .HasForeignKey(d => d.ProductoId);
    }
}