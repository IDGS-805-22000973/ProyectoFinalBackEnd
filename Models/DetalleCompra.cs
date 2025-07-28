namespace ProyectoFinal.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }
        public int CompraId { get; set; }
        public Compra Compra { get; set; }

        public int MateriaPrimaId { get; set; }
        public MateriaPrima MateriaPrima { get; set; }

        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        public decimal Subtotal => Cantidad * CostoUnitario;
    }
}