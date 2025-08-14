// En ProyectoFinal/Dtos/VentaDto.cs
namespace ProyectoFinal.Dtos
{
    public class CrearVentaAdminDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string ClienteId { get; set; } // ID del cliente al que se asigna la venta
    }

    public class VentaClienteDto
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public ProductoDto Producto { get; set; }
        public UsuarioDto? CreadoPorAdmin { get; set; }
    }

    public class VentaAdminDto
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public ProductoDto Producto { get; set; }
        public UsuarioDto? Cliente { get; set; }
        public string? CreadoPor { get; set; }
    }
}