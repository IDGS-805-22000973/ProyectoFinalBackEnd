using ProyectoFinal.Models;

public class Cotizacion
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Empresa { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public List<ItemCotizacion> Productos { get; set; } = new List<ItemCotizacion>();
    public decimal Total { get; set; }
}

public class ItemCotizacion
{
    public int Id { get; set; }
    public int CotizacionId { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}