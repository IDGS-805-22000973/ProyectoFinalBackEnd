namespace ProyectoFinal.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }

        public List<ComponenteProducto> Componentes { get; set; } = new();
    }
}