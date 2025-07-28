using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(100)]
        public string? Empresa { get; set; }

        [MaxLength(100)]
        public string? Correo { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public string? Direccion { get; set; }

        public List<Compra> Compras { get; set; } = new();
    }
}