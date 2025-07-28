using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/proveedores
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var proveedores = await _context.Proveedores
                .Select(p => new ProveedorDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Empresa = p.Empresa,
                    Correo = p.Correo,
                    Telefono = p.Telefono,
                    Direccion = p.Direccion
                }).ToListAsync();

            return Ok(proveedores);
        }

        // GET: api/proveedores/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Proveedores.FindAsync(id);
            if (p == null) return NotFound();

            return Ok(new ProveedorDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Empresa = p.Empresa,
                Correo = p.Correo,
                Telefono = p.Telefono,
                Direccion = p.Direccion
            });
        }

        // POST: api/proveedores
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearProveedorDto dto)
        {
            var proveedor = new Proveedor
            {
                Nombre = dto.Nombre,
                Empresa = dto.Empresa,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor creado correctamente", proveedor.Id });
        }

        // PUT: api/proveedores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CrearProveedorDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();

            proveedor.Nombre = dto.Nombre;
            proveedor.Empresa = dto.Empresa;
            proveedor.Correo = dto.Correo;
            proveedor.Telefono = dto.Telefono;
            proveedor.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor actualizado" });
        }

        // DELETE: api/proveedores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var proveedor = await _context.Proveedores
                .Include(p => p.Compras) // Validación de integridad
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
                return NotFound();

            if (proveedor.Compras.Any())
                return BadRequest(new { mensaje = "No se puede eliminar proveedor con compras registradas" });

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor eliminado correctamente" });
        }
    }
}
