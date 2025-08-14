namespace ProyectoFinal.Dtos
{
    public class CrearProductoDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        //public decimal PrecioVenta { get; set; }
        public List<ComponenteDto> Componentes { get; set; }
    }

    public class ActualizarProductoDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public List<ComponenteDto> Componentes { get; set; }
    }

    public class ActualizarPrecioDto
    {
        public decimal PrecioVenta { get; set; }
    }

    public class ComponenteDto
    {
        public int MateriaPrimaId { get; set; }
        public int Cantidad { get; set; }
    }
}