using System.ComponentModel.DataAnnotations;

public class ItemCotizacionDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class CrearCotizacionDto
{
    [Required]
    public string NombreCompleto { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Telefono { get; set; }

    public string Empresa { get; set; }

    [Required]
    public List<ItemCotizacionDto> Productos { get; set; } = new List<ItemCotizacionDto>();
}

public class CotizacionDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Empresa { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<ItemCotizacionDto> Productos { get; set; }
    public decimal Total { get; set; }
}