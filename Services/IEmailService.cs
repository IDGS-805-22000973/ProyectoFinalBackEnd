using ProyectoFinal.Models;

namespace ProyectoFinal.Services
{
    public interface IEmailService
    {
        Task EnviarCorreosCotizacionAsync(Cotizacion cotizacion);

        Task EnviarCredencialesUsuarioAsync(string nombre, string email, string password);
        Task EnviarConfirmacionVentaAsync(Venta venta, AppUser cliente, Producto producto);

    }
}
