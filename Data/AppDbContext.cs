using ProyectoFinal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProyectoFinal.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<MateriaPrima> MateriaPrima { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ComponenteProducto> ComponentesProducto { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }




        // Puedes agregar DbSet adicionales si deseas
    }
}
