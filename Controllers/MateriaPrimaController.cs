using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //
    public class MateriaPrimaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MateriaPrimaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var materias = await _context.MateriaPrima.ToListAsync();
            return Ok(materias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var materia = await _context.MateriaPrima.FindAsync(id);
            if (materia == null)
                return NotFound(new { mensaje = "Materia prima no encontrada" });

            return Ok(materia);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MateriaPrima model)
        {
            _context.MateriaPrima.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Materia prima creada", model.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MateriaPrima model)
        {
            var materia = await _context.MateriaPrima.FindAsync(id);
            if (materia == null)
                return NotFound(new { mensaje = "Materia prima no encontrada" });

            materia.Nombre = model.Nombre;
            materia.Unidad = model.Unidad;
            materia.PorcentajeGanancia = model.PorcentajeGanancia;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Materia prima actualizada" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var materia = await _context.MateriaPrima.FindAsync(id);
            if (materia == null)
                return NotFound(new { mensaje = "Materia prima no encontrada" });

            _context.MateriaPrima.Remove(materia);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Materia prima eliminada" });
        }
    }
}
