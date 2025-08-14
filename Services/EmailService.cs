using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ProyectoFinal.Models;
using ProyectoFinal.Settings;
using System.Text;

namespace ProyectoFinal.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        // MÉTODO ORQUESTADOR PRINCIPAL
        public async Task EnviarCorreosCotizacionAsync(Cotizacion cotizacion)
        {
            try
            {
                // 1. Crear el correo para el CLIENTE
                var correoCliente = new MimeMessage();
                correoCliente.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                correoCliente.To.Add(new MailboxAddress(cotizacion.NombreCompleto, cotizacion.Email));
                correoCliente.Subject = $"Confirmación de Cotización #{cotizacion.Id}";
                var bodyCliente = new BodyBuilder { HtmlBody = GenerarCuerpoCorreoCliente(cotizacion) }; // Se llama a GenerarCuerpoCorreoCliente
                correoCliente.Body = bodyCliente.ToMessageBody();

                // 2. Crear el correo para la NOTIFICACIÓN INTERNA
                var correoInterno = new MimeMessage();
                correoInterno.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                correoInterno.To.Add(new MailboxAddress("Notificaciones Water-Life", _smtpSettings.SenderEmail));
                correoInterno.Subject = $"✅ Nueva Cotización Recibida - ID: {cotizacion.Id}";
                var bodyInterno = new BodyBuilder { HtmlBody = GenerarCuerpoNotificacionInterna(cotizacion) };
                correoInterno.Body = bodyInterno.ToMessageBody();

                // 3. Conectar y enviar AMBOS correos
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

                    // Enviar los dos correos en la misma conexión
                    await client.SendAsync(correoCliente);
                    _logger.LogInformation($"Correo de cotización {cotizacion.Id} enviado a {cotizacion.Email}.");

                    await client.SendAsync(correoInterno);
                    _logger.LogInformation($"Notificación interna para cotización {cotizacion.Id} enviada.");

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el proceso de envío de correos para la cotización {CotizacionId}", cotizacion.Id);
            }
        }


        public async Task EnviarConfirmacionVentaAsync(Venta venta, AppUser cliente, Producto producto)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                message.To.Add(new MailboxAddress(cliente.Nombre, cliente.Email));
                message.Subject = $"¡Gracias por tu compra en Water-Life! - Pedido #{venta.Id}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GenerarCuerpoConfirmacionVenta(venta, cliente, producto)
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                _logger.LogInformation($"Correo de confirmación de venta #{venta.Id} enviado a {cliente.Email}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar confirmación de venta #{VentaId} al usuario {Email}", venta.Id, cliente.Email);
            }
        }


        public async Task EnviarCredencialesUsuarioAsync(string nombre, string email, string password)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                message.To.Add(new MailboxAddress(nombre, email));
                message.Subject = "¡Bienvenido/a a Water-Life! Tus Credenciales de Acceso";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GenerarCuerpoCorreoCredenciales(nombre, email, password)
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                _logger.LogInformation($"Correo de bienvenida enviado exitosamente a {email}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de bienvenida al usuario {Email}", email);
            }
        }



        // CORRECCIÓN: El nombre del método ahora coincide con la llamada de arriba.
        private string GenerarCuerpoCorreoCliente(Cotizacion cotizacion)
        {
            var sb = new StringBuilder();
            sb.Append($"<h1>Hola {cotizacion.NombreCompleto},</h1>");
            sb.Append($"<p>Gracias por solicitar una cotización con nosotros. Aquí están los detalles:</p>");
            sb.Append($"<p><strong>Fecha:</strong> {cotizacion.FechaCreacion:dd/MM/yyyy}</p>");

            sb.Append("<table border='1' cellpadding='5' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<thead><tr style='background-color: #f2f2f2;'><th>Producto</th><th>Cantidad</th><th>Precio Unitario</th><th>Subtotal</th></tr></thead>");
            sb.Append("<tbody>");

            foreach (var item in cotizacion.Productos)
            {
                sb.Append($"<tr><td>{item.Producto.Nombre}</td><td>{item.Cantidad}</td><td>${item.PrecioUnitario:N2}</td><td>${(item.Cantidad * item.PrecioUnitario):N2}</td></tr>");
            }

            sb.Append("</tbody></table>");

            sb.Append($"<h2 style='text-align: right;'>Total: ${cotizacion.Total:N2}</h2>");
            sb.Append("<p>Si tienes alguna pregunta, no dudes en contactarnos.</p>");
            sb.Append("<p>Saludos,<br>Por parte de Water-Life</p>");

            return sb.ToString();
        }


        private string GenerarCuerpoNotificacionInterna(Cotizacion cotizacion)
        {
            var sb = new StringBuilder();
            sb.Append("<h1>¡Se ha generado una nueva solicitud de cotización!</h1>");
            sb.Append("<p>Un cliente ha solicitado una cotización a través del sistema. A continuación los detalles para dar seguimiento:</p>");

            sb.Append("<h2>Datos del Cliente</h2>");
            sb.Append($"<ul>");
            sb.Append($"<li><strong>Nombre:</strong> {cotizacion.NombreCompleto}</li>");
            sb.Append($"<li><strong>Email:</strong> {cotizacion.Email}</li>");
            sb.Append($"<li><strong>Teléfono:</strong> {cotizacion.Telefono}</li>");
            sb.Append($"<li><strong>Empresa:</strong> {(string.IsNullOrEmpty(cotizacion.Empresa) ? "No especificada" : cotizacion.Empresa)}</li>");
            sb.Append($"</ul>");

            sb.Append("<h2>Detalles de la Cotización</h2>");
            sb.Append($"<p><strong>ID de Cotización:</strong> {cotizacion.Id}</p>");
            sb.Append("<table border='1' cellpadding='5' style='border-collapse: collapse; width: 100%;'>");
            sb.Append("<thead><tr style='background-color: #f2f2f2;'><th>Producto</th><th>Cantidad</th><th>Precio Unitario</th><th>Subtotal</th></tr></thead>");
            sb.Append("<tbody>");

            foreach (var item in cotizacion.Productos)
            {
                sb.Append($"<tr><td>{item.Producto.Nombre}</td><td>{item.Cantidad}</td><td>${item.PrecioUnitario:N2}</td><td>${(item.Cantidad * item.PrecioUnitario):N2}</td></tr>");
            }

            sb.Append("</tbody></table>");
            sb.Append($"<h2 style='text-align: right;'>Total: ${cotizacion.Total:N2}</h2>");

            return sb.ToString();
        }


        private string GenerarCuerpoCorreoCredenciales(string nombre, string email, string password)
        {
            var sb = new StringBuilder();
            sb.Append($"<h1>¡Hola {nombre}, bienvenido/a a Water-Life!</h1>");
            sb.Append("<p>Tu cuenta ha sido creada exitosamente. Ya puedes acceder a nuestro sistema con las siguientes credenciales:</p>");

            sb.Append("<div style='border: 1px solid #ddd; padding: 15px; background-color: #f9f9f9;'>");
            sb.Append($"<p><strong>Usuario:</strong> {email}</p>");
            sb.Append($"<p><strong>Contraseña Temporal:</strong> {password}</p>");
            sb.Append("</div>");

            sb.Append("<p style='margin-top: 20px;'><strong>Importante:</strong> Por tu seguridad, te recomendamos encarecidamente que <strong>cambies tu contraseña</strong> después de iniciar sesión por primera vez.</p>");
            sb.Append("<p>¡Estamos contentos de tenerte con nosotros!</p>");
            sb.Append("<p>Saludos,<br>El equipo de Water-Life</p>");

            return sb.ToString();
        }

        private string GenerarCuerpoConfirmacionVenta(Venta venta, AppUser cliente, Producto producto)
        {
            var sb = new StringBuilder();
            sb.Append($"<h1>¡Gracias por tu compra, {cliente.Nombre}!</h1>");
            sb.Append($"<p>Hemos recibido y procesado tu pedido. Aquí tienes los detalles de tu compra:</p>");

            sb.Append("<h2>Resumen del Pedido</h2>");
            sb.Append("<table border='1' cellpadding='10' style='border-collapse: collapse; width: 100%;'>");
            sb.Append($"<tr><td style='background-color: #f2f2f2;'><strong>Número de Pedido:</strong></td><td>{venta.Id}</td></tr>");
            sb.Append($"<tr><td style='background-color: #f2f2f2;'><strong>Fecha de Compra:</strong></td><td>{venta.FechaVenta:dd/MM/yyyy HH:mm}</td></tr>");
            sb.Append("</table>");

            sb.Append("<h3 style='margin-top: 20px;'>Artículos Comprados</h3>");
            sb.Append("<table border='1' cellpadding='10' style='border-collapse: collapse; width: 100%; text-align: left;'>");
            sb.Append("<thead style='background-color: #f2f2f2;'><tr><th>Producto</th><th>Cantidad</th><th>Precio Unitario</th><th>Total</th></tr></thead>");
            sb.Append("<tbody>");
            sb.Append($"<tr><td>{producto.Nombre}</td><td>{venta.Cantidad}</td><td>${venta.PrecioUnitario:N2}</td><td>${venta.Total:N2}</td></tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");

            sb.Append($"<h2 style='text-align: right; margin-top: 20px;'>Total Pagado: ${venta.Total:N2}</h2>");

            sb.Append("<p style='margin-top: 30px;'>Si tienes alguna pregunta sobre tu pedido, no dudes en contactarnos.</p>");
            sb.Append("<p>Atentamente,<br>El equipo de Water-Life</p>");

            return sb.ToString();
        }
    }
}