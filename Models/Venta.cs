// En ProyectoFinal/Models/Venta.cs
namespace ProyectoFinal.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } // "Completada", "Pendiente", "Cancelada", etc.

        // Relaciones
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        public string UsuarioId { get; set; } // Cliente
        public AppUser Usuario { get; set; }

        public string? CreadoPorAdminId { get; set; } // Admin que creó la venta
        public AppUser? CreadoPorAdmin { get; set; }

    }
}