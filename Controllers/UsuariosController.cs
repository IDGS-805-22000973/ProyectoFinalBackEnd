using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuariosController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Crear usuario con rol
        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest(new { mensaje = "El correo ya está registrado" });

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Nombre = dto.Nombre
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { mensaje = "Error al crear usuario", errores = result.Errors });

            if (!await _roleManager.RoleExistsAsync(dto.Rol))
                await _roleManager.CreateAsync(new IdentityRole(dto.Rol));

            await _userManager.AddToRoleAsync(user, dto.Rol);

            return Ok(new { mensaje = "Usuario creado correctamente", email = user.Email });
        }

        // 2. Obtener todos los usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = _userManager.Users.ToList();
            var resultado = new List<UsuarioDto>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);
                resultado.Add(new UsuarioDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return Ok(resultado);
        }

        // 3. Obtener usuario por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UsuarioDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Roles = roles.ToList()
            });
        }

        // 4. Actualizar nombre y email
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(string id, [FromBody] ActualizarUsuarioDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            user.Nombre = dto.Nombre;
            user.Email = dto.Email;
            user.UserName = dto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { mensaje = "Error al actualizar usuario", errores = result.Errors });

            return Ok(new { mensaje = "Usuario actualizado correctamente" });
        }

        // 5. Cambiar rol
        [HttpPut("cambiar-rol")]
        public async Task<IActionResult> CambiarRol([FromBody] ActualizarRolDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            if (!await _roleManager.RoleExistsAsync(dto.NuevoRol))
                await _roleManager.CreateAsync(new IdentityRole(dto.NuevoRol));

            await _userManager.AddToRoleAsync(user, dto.NuevoRol);

            return Ok(new { mensaje = $"Rol actualizado a {dto.NuevoRol}" });
        }

        // 6. Eliminar usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            await _userManager.DeleteAsync(user);
            return Ok(new { mensaje = "Usuario eliminado correctamente" });
        }
    }
}