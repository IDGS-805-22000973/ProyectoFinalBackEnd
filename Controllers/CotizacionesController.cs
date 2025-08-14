using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;
using ProyectoFinal.Services; // ¡Importante! Agregar el using del servicio

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CotizacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService; // 1. Declara el servicio

        // 2. Inyéctalo en el constructor
        public CotizacionesController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearCotizacion([FromBody] CrearCotizacionDto dto)
        {
            try
            {
                // ... (toda tu lógica de validación actual se mantiene igual)
                if (string.IsNullOrEmpty(dto.NombreCompleto)) return BadRequest("Nombre completo es requerido");
                // ... etc.

                decimal total = 0;
                var productosCotizacion = new List<ItemCotizacion>();

                foreach (var item in dto.Productos)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null) return BadRequest($"Producto con ID {item.ProductoId} no encontrado");
                    if (item.Cantidad <= 0) return BadRequest($"Cantidad para producto {producto.Nombre} debe ser mayor a cero");

                    total += producto.PrecioVenta * item.Cantidad;

                    productosCotizacion.Add(new ItemCotizacion
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.PrecioVenta,
                        Producto = producto // Importante: Asigna el producto para tener el nombre en el email
                    });
                }

                var cotizacion = new Cotizacion
                {
                    NombreCompleto = dto.NombreCompleto,
                    Email = dto.Email,
                    Telefono = dto.Telefono,
                    Empresa = dto.Empresa,
                    FechaCreacion = DateTime.UtcNow,
                    Productos = productosCotizacion,
                    Total = total
                };

                _context.Cotizaciones.Add(cotizacion);
                await _context.SaveChangesAsync();

                // ¡ÚNICA LLAMADA AL SERVICIO DE CORREO!
                await _emailService.EnviarCorreosCotizacionAsync(cotizacion);

                return Ok(new
                {
                    mensaje = "Cotización creada y enviada por correo exitosamente",
                    cotizacionId = cotizacion.Id,
                    total
                });
            
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { mensaje = "Error al guardar en base de datos", error = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
            }
        }
    }
}