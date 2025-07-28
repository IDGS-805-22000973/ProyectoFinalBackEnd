namespace ProyectoFinal.Models
{
    public class MateriaPrima
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; } // Ej: kg, litros, piezas

        public int Existencia { get; set; }
        public decimal CostoPromedio { get; set; }
        public decimal PorcentajeGanancia { get; set; }

        public List<DetalleCompra> DetallesCompra { get; set; } = new();
        public List<ComponenteProducto> ComponentesProducto { get; set; } = new();
    }
}