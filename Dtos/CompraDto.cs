namespace ProyectoFinal.Dtos
{
    public class CompraDto
    {
        public int Id { get; set; }
        public DateTime FechaCompra { get; set; }
        public string ProveedorNombre { get; set; }
        public decimal Total { get; set; }
    }
}