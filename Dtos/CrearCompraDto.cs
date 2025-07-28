namespace ProyectoFinal.Dtos
{
    public class CrearCompraDto
    {
        public int ProveedorId { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; }
    }
        
    public class DetalleCompraDto
    {
        public int MateriaPrimaId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}
