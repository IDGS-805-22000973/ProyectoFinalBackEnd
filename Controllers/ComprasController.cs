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
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompras()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                .Select(c => new CompraDto
                {
                    Id = c.Id,
                    FechaCompra = c.FechaCompra,
                    ProveedorNombre = c.Proveedor.Nombre,
                    Total = c.Detalles.Sum(d => d.CostoUnitario * d.Cantidad)
                })
                .ToListAsync();

            return Ok(compras);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCompra([FromBody] CrearCompraDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
            if (proveedor == null) return BadRequest(new { mensaje = "Proveedor no encontrado" });

            var compra = new Compra
            {
                ProveedorId = dto.ProveedorId,
                FechaCompra = DateTime.Now,
                Detalles = new List<DetalleCompra>()
            };

            foreach (var detalle in dto.Detalles)
            {
                var materia = await _context.MateriaPrima.FindAsync(detalle.MateriaPrimaId);
                if (materia == null) return BadRequest(new { mensaje = $"Materia prima ID {detalle.MateriaPrimaId} no encontrada" });

                // Recalcular Costo Promedio
                var totalAnterior = materia.Existencia * materia.CostoPromedio;
                var totalNuevo = detalle.Cantidad * detalle.CostoUnitario;
                var nuevaExistencia = materia.Existencia + detalle.Cantidad;

                materia.CostoPromedio = (totalAnterior + totalNuevo) / nuevaExistencia;
                materia.Existencia = nuevaExistencia;

                compra.Detalles.Add(new DetalleCompra
                {
                    MateriaPrimaId = detalle.MateriaPrimaId,
                    Cantidad = detalle.Cantidad,
                    CostoUnitario = detalle.CostoUnitario
                });
            }

            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Compra registrada y costos actualizados", compra.Id });
        }


        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumenCompras()
        {
            var ultimoMes = DateTime.Now.AddMonths(-1);

            var resumen = new
            {
                totalUltimoMes = await _context.Compras
                    .Where(c => c.FechaCompra >= ultimoMes)
                    .SumAsync(c => c.Detalles.Sum(d => d.CostoUnitario * d.Cantidad)),
                comprasPorMes = await _context.Compras
                    .GroupBy(c => new { c.FechaCompra.Year, c.FechaCompra.Month })
                    .Select(g => new {
                        mes = $"{g.Key.Year}-{g.Key.Month}",
                        total = g.Sum(c => c.Detalles.Sum(d => d.CostoUnitario * d.Cantidad))
                    })
                    .ToListAsync()
            };

            return Ok(resumen);
        }

        [HttpGet("recientes")]
        public async Task<IActionResult> GetComprasRecientes()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                .OrderByDescending(c => c.FechaCompra)
                .Take(5)
                .Select(c => new CompraDto
                {
                    Id = c.Id,
                    FechaCompra = c.FechaCompra,
                    ProveedorNombre = c.Proveedor.Nombre,
                    Total = c.Detalles.Sum(d => d.CostoUnitario * d.Cantidad)
                })
                .ToListAsync();

            return Ok(compras);
        }
    }
}
