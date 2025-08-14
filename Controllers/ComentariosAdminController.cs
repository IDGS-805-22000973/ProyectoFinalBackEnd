using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Data;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;
using System.Security.Claims;

namespace ProyectoFinal.Controllers
{
    // Nuevo archivo: AdminController.cs o extensión del existente
    //[Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    public class ComentariosAdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ComentariosAdminController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Obtener todos los comentarios
        [HttpGet("todos-comentarios")]
        public async Task<ActionResult<IEnumerable<ComentarioAdminDto>>> GetAllComentarios()
        {
            var comentarios = await _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.AdminRespuesta)
                .OrderByDescending(c => c.FechaCreacion)
                .Select(c => new ComentarioAdminDto
                {
                    Id = c.Id,
                    Texto = c.Texto,
                    FechaCreacion = c.FechaCreacion,
                    UsuarioNombre = c.Usuario.Nombre,
                    Respuesta = c.Respuesta,
                    FechaRespuesta = c.FechaRespuesta,
                    AdminRespuestaNombre = c.AdminRespuesta != null ? c.AdminRespuesta.Nombre : null
                })
                .ToListAsync();

            return Ok(comentarios);
        }

        // Responder a un comentario
        [HttpPost("responder-comentario")]
        public async Task<IActionResult> ResponderComentario([FromBody] ResponderComentarioDto model)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = await _userManager.FindByIdAsync(adminId);

            if (admin == null)
            {
                return Unauthorized(new { message = "Admin no encontrado" });
            }

            var comentario = await _context.Comentarios.FindAsync(model.ComentarioId);
            if (comentario == null)
            {
                return NotFound(new { message = "Comentario no encontrado" });
            }

            comentario.Respuesta = model.Respuesta;
            comentario.AdminRespuestaId = adminId;
            comentario.FechaRespuesta = DateTime.UtcNow;

            _context.Comentarios.Update(comentario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Respuesta agregada correctamente" });
        }

        // Eliminar un comentario
        [HttpDelete("eliminar-comentario/{id}")]
        public async Task<IActionResult> EliminarComentario(int id)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null)
            {
                return NotFound(new { message = "Comentario no encontrado" });
            }

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comentario eliminado correctamente" });
        }
    }
}
