using ProyectoFinal.Models;

namespace ProyectoFinal.Dtos
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public List<ComponenteProducto> Componentes { get; set; } = new();
    }
}