using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;
using System.Security.Claims;

namespace ProyectoFinal.Controllers
{
    [Authorize(Roles = "Cliente")]
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public ClienteController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Endpoint para obtener información del usuario actual
        [HttpGet("mi-perfil")]
        public async Task<ActionResult<UsuarioDto>> GetMiPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UsuarioDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Roles = roles.ToList()
            });
        }

        // Endpoint para actualizar el nombre del usuario
        [HttpPut("actualizar-nombre")]
        public async Task<IActionResult> ActualizarNombre([FromBody] ClienteDTO.ActualizarNombreDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            user.Nombre = model.Nombre;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Error al actualizar el nombre", errors = result.Errors });
            }

            return Ok(new { message = "Nombre actualizado correctamente" });
        }

        // Endpoint para actualizar el email del usuario
        [HttpPut("actualizar-email")]
        public async Task<IActionResult> ActualizarEmail([FromBody] ClienteDTO.ActualizarEmailDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Verificar si el nuevo email ya está en uso
            var existingUser = await _userManager.FindByEmailAsync(model.NuevoEmail);
            if (existingUser != null && existingUser.Id != userId)
            {
                return BadRequest(new { message = "El correo electrónico ya está en uso por otra cuenta" });
            }

            // Cambiar el email
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.NuevoEmail);
            var result = await _userManager.ChangeEmailAsync(user, model.NuevoEmail, token);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Error al actualizar el email", errors = result.Errors });
            }

            // Actualizar también el nombre de usuario si es el mismo que el email
            if (user.UserName == user.Email)
            {
                user.UserName = model.NuevoEmail;
                await _userManager.UpdateAsync(user);
            }

            return Ok(new { message = "Email actualizado correctamente" });
        }

        // Endpoint para cambiar la contraseña
        [HttpPut("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] ClienteDTO.CambiarPasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.PasswordActual, model.NuevoPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Error al cambiar la contraseña", errors = result.Errors });
            }

            return Ok(new { message = "Contraseña cambiada correctamente" });
        }

        // Endpoints existentes para compras y productos...
        [HttpGet("mis-compras")]
        public async Task<ActionResult<IEnumerable<VentaClienteDto>>> GetMisCompras()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var ventas = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .Include(v => v.Producto)
                .Include(v => v.CreadoPorAdmin)
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new VentaClienteDto
                {
                    Id = v.Id,
                    FechaVenta = v.FechaVenta,
                    Cantidad = v.Cantidad,
                    PrecioUnitario = v.PrecioUnitario,
                    Total = v.Total,
                    Estado = v.Estado,
                    Producto = new ProductoDto
                    {
                        Id = v.Producto.Id,
                        Nombre = v.Producto.Nombre,
                        Descripcion = v.Producto.Descripcion,
                        PrecioVenta = v.Producto.PrecioVenta
                    },
                    CreadoPorAdmin = v.CreadoPorAdmin != null ? new UsuarioDto
                    {
                        Id = v.CreadoPorAdmin.Id,
                        Nombre = v.CreadoPorAdmin.Nombre,
                        Email = v.CreadoPorAdmin.Email
                    } : null
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("mis-productos")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosAdquiridos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var productosAdquiridos = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .Include(v => v.Producto)
                .Select(v => new ProductoDto
                {
                    Id = v.Producto.Id,
                    Nombre = v.Producto.Nombre,
                    Descripcion = v.Producto.Descripcion,
                    PrecioVenta = v.Producto.PrecioVenta
                })
                .Distinct()
                .ToListAsync();

            return Ok(productosAdquiridos);
        }



        // En ClienteController.cs

        // Endpoint para crear un nuevo comentario
        [HttpPost("crear-comentario")]
        public async Task<ActionResult<ComentarioDto>> CrearComentario([FromBody] CrearComentarioDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Validación adicional del modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var comentario = new Comentario
                {
                    UsuarioId = userId,
                    Texto = model.Texto, // Asegúrate que coincida con el nombre en el DTO
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();

                return Ok(new ComentarioDto
                {
                    Id = comentario.Id,
                    Texto = comentario.Texto,
                    FechaCreacion = comentario.FechaCreacion,
                    UsuarioNombre = user.Nombre
                });
            }
            catch (DbUpdateException ex)
            {
                // Loggear el error si es necesario
                Console.WriteLine($"Error al guardar comentario: {ex.InnerException?.Message}");
                return StatusCode(500, new { message = "Error al guardar el comentario", error = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Endpoint para obtener todos los comentarios del usuario
        [HttpGet("mis-comentarios")]
        public async Task<ActionResult<IEnumerable<ComentarioDto>>> GetMisComentarios()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comentarios = await _context.Comentarios
                .Where(c => c.UsuarioId == userId)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaCreacion)
                .Select(c => new ComentarioDto
                {
                    Id = c.Id,
                    Texto = c.Texto,
                    FechaCreacion = c.FechaCreacion,
                    UsuarioNombre = c.Usuario.Nombre
                })
                .ToListAsync();

            return Ok(comentarios);
        }
    }
}