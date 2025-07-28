using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public DateTime FechaCompra { get; set; } = DateTime.Now;

        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; }

        public List<DetalleCompra> Detalles { get; set; } = new();
    }
}