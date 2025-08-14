using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Data;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;
using ProyectoFinal.Services;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        // 2. DECLARA LA VARIABLE PARA EL SERVICIO DE CORREO
        private readonly IEmailService _emailService;

        // 3. INYECTA EL SERVICIO EN EL CONSTRUCTOR
        public UsuariosController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService; // Asigna el servicio
        }

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

            // La contraseña en texto plano solo existe aquí (en dto.Password)
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { mensaje = "Error al crear usuario", errores = result.Errors });

            if (!await _roleManager.RoleExistsAsync(dto.Rol))
                await _roleManager.CreateAsync(new IdentityRole(dto.Rol));

            await _userManager.AddToRoleAsync(user, dto.Rol);

            // 4. LLAMA AL SERVICIO DE CORREO DESPUÉS DE CREAR EL USUARIO
            // Enviamos el dto.Password porque es la única vez que lo tenemos en texto plano
            await _emailService.EnviarCredencialesUsuarioAsync(user.Nombre, user.Email, dto.Password);

            return Ok(new { mensaje = "Usuario creado y notificado por correo correctamente", email = user.Email });
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

        [HttpGet("clientes")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerClientes()
        {
            var clientes = await _userManager.GetUsersInRoleAsync("Cliente");
            return Ok(clientes.Select(c => new UsuarioDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email
            }));
        }

       
    }
}