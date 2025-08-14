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
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] CrearProductoDto dto)
        {
            var componentes = new List<ComponenteProducto>();
            decimal precioCalculado = 0;

            foreach (var comp in dto.Componentes)
            {
                var materia = await _context.MateriaPrima.FindAsync(comp.MateriaPrimaId);
                if (materia == null)
                    return BadRequest(new { mensaje = $"Materia prima ID {comp.MateriaPrimaId} no encontrada" });

                // Calculamos el precio basado en: (cantidad * costo * (1 + %ganancia))
                precioCalculado += comp.Cantidad * materia.CostoPromedio * (1 + (materia.PorcentajeGanancia / 100));

                componentes.Add(new ComponenteProducto
                {
                    MateriaPrimaId = comp.MateriaPrimaId,
                    Cantidad = comp.Cantidad
                });
            }

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PrecioVenta = precioCalculado, // Asignamos el precio calculado automáticamente
                Componentes = componentes
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Producto creado correctamente",
                producto.Id,
                PrecioCalculado = precioCalculado,
                Nota = "Puede modificar el precio después usando el endpoint PATCH api/productos/{id}/precio"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _context.Productos
                .Include(p => p.Componentes)
                .ThenInclude(cp => cp.MateriaPrima)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    p.PrecioVenta,
                    PrecioSugerido = p.Componentes.Sum(c => c.Cantidad * c.MateriaPrima.CostoPromedio * (1 + (c.MateriaPrima.PorcentajeGanancia / 100))),
                    Componentes = p.Componentes.Select(c => new
                    {
                        c.MateriaPrimaId,
                        NombreMateriaPrima = c.MateriaPrima.Nombre,
                        c.Cantidad,
                        CostoUnitario = c.MateriaPrima.CostoPromedio,
                        PorcentajeGanancia = c.MateriaPrima.PorcentajeGanancia,
                        ContribucionPrecio = c.Cantidad * c.MateriaPrima.CostoPromedio * (1 + (c.MateriaPrima.PorcentajeGanancia / 100))
                    }).ToList()
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductoById(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Componentes)
                .ThenInclude(cp => cp.MateriaPrima)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    p.PrecioVenta,
                    PrecioSugerido = p.Componentes.Sum(c => c.Cantidad * c.MateriaPrima.CostoPromedio * (1 + (c.MateriaPrima.PorcentajeGanancia / 100))),
                    Componentes = p.Componentes.Select(c => new
                    {
                        c.MateriaPrimaId,
                        NombreMateriaPrima = c.MateriaPrima.Nombre,
                        c.Cantidad,
                        CostoUnitario = c.MateriaPrima.CostoPromedio,
                        PorcentajeGanancia = c.MateriaPrima.PorcentajeGanancia,
                        ContribucionPrecio = c.Cantidad * c.MateriaPrima.CostoPromedio * (1 + (c.MateriaPrima.PorcentajeGanancia / 100))
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound(new { mensaje = $"Producto con ID {id} no encontrado" });
            }

            return Ok(producto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarProducto(int id, [FromBody] ActualizarProductoDto dto)
        {
            var productoExistente = await _context.Productos
                .Include(p => p.Componentes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productoExistente == null)
            {
                return NotFound(new { mensaje = $"Producto con ID {id} no encontrado" });
            }

            // Calcular nuevo precio basado en componentes
            decimal nuevoPrecio = 0;
            foreach (var comp in dto.Componentes)
            {
                var materia = await _context.MateriaPrima.FindAsync(comp.MateriaPrimaId);
                if (materia == null)
                    return BadRequest(new { mensaje = $"Materia prima ID {comp.MateriaPrimaId} no encontrada" });

                nuevoPrecio += comp.Cantidad * materia.CostoPromedio * (1 + (materia.PorcentajeGanancia / 100));
            }

            // Eliminar componentes existentes
            _context.ComponentesProducto.RemoveRange(productoExistente.Componentes);

            // Crear nuevos componentes
            var componentes = new List<ComponenteProducto>();
            foreach (var comp in dto.Componentes)
            {
                componentes.Add(new ComponenteProducto
                {
                    MateriaPrimaId = comp.MateriaPrimaId,
                    Cantidad = comp.Cantidad,
                    ProductoId = id
                });
            }

            // Actualizar propiedades del producto
            productoExistente.Nombre = dto.Nombre ?? productoExistente.Nombre;
            productoExistente.Descripcion = dto.Descripcion ?? productoExistente.Descripcion;
            productoExistente.PrecioVenta = nuevoPrecio; // Usamos el precio calculado automáticamente
            productoExistente.Componentes = componentes;

            _context.Productos.Update(productoExistente);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Producto modificado correctamente",
                id,
                PrecioCalculado = nuevoPrecio,
                Nota = "El precio se actualizó automáticamente basado en los componentes. Puede ajustarlo después con PATCH api/productos/{id}/precio si lo desea."
            });
        }

        // Los demás métodos (ActualizarPrecio, EliminarProducto) permanecen igual

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Componentes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                return NotFound(new { mensaje = $"Producto con ID {id} no encontrado" });
            }

            // Eliminar primero los componentes asociados
            _context.ComponentesProducto.RemoveRange(producto.Componentes);

            // Luego eliminar el producto
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto eliminado correctamente", id });
        }
    }
}