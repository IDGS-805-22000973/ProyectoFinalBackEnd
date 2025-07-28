using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { mensaje = "Credenciales incorrectas" });
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var rolPrincipal = userRoles.FirstOrDefault() ?? "SinRol";

            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName!),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, rolPrincipal)
    };

            var authSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWTSettings:securityKey"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["JWTSettings:ValidIssuer"],
                audience: _configuration["JWTSettings:ValidAudience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JWTSettings:expireInMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracion = token.ValidTo,
                NombreUsuario = user.Nombre,
                Rol = rolPrincipal,
                Mensaje = $"Bienvenido {user.Nombre}, has iniciado sesión como {rolPrincipal}"
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return BadRequest(new { mensaje = "El correo ya está registrado" });

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(new { mensaje = "Error al registrar usuario", errores = result.Errors });

            if (!await _roleManager.RoleExistsAsync("Cliente"))
                await _roleManager.CreateAsync(new IdentityRole("Cliente"));

            await _userManager.AddToRoleAsync(user, "Cliente");

            return Ok(new RegisterResponseDto
            {
                Mensaje = "Usuario registrado correctamente",
                Email = user.Email!
            });
        }
    }
}
