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
            if (dto.PrecioVenta <= 0)
                return BadRequest(new { mensaje = "El precio de venta debe ser mayor que cero" });

            var componentes = new List<ComponenteProducto>();

            foreach (var comp in dto.Componentes)
            {
                var materia = await _context.MateriaPrima.FindAsync(comp.MateriaPrimaId);
                if (materia == null)
                    return BadRequest(new { mensaje = $"Materia prima ID {comp.MateriaPrimaId} no encontrada" });

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
                PrecioVenta = dto.PrecioVenta,
                Componentes = componentes
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto creado correctamente", producto.Id });
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
                    Componentes = p.Componentes.Select(c => new
                    {
                        c.MateriaPrimaId,
                        NombreMateriaPrima = c.MateriaPrima.Nombre,
                        c.Cantidad
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
                    Componentes = p.Componentes.Select(c => new
                    {
                        c.MateriaPrimaId,
                        NombreMateriaPrima = c.MateriaPrima.Nombre,
                        c.Cantidad
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

            if (dto.PrecioVenta <= 0)
                return BadRequest(new { mensaje = "El precio de venta debe ser mayor que cero" });

            // Eliminar componentes existentes
            _context.ComponentesProducto.RemoveRange(productoExistente.Componentes);

            // Crear nuevos componentes
            var componentes = new List<ComponenteProducto>();

            foreach (var comp in dto.Componentes)
            {
                var materia = await _context.MateriaPrima.FindAsync(comp.MateriaPrimaId);
                if (materia == null)
                    return BadRequest(new { mensaje = $"Materia prima ID {comp.MateriaPrimaId} no encontrada" });

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
            productoExistente.PrecioVenta = dto.PrecioVenta;
            productoExistente.Componentes = componentes;

            _context.Productos.Update(productoExistente);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto modificado correctamente", id });
        }

        [HttpPatch("{id}/precio")]
        public async Task<IActionResult> ActualizarPrecio(int id, [FromBody] ActualizarPrecioDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new { mensaje = $"Producto con ID {id} no encontrado" });
            }

            if (dto.PrecioVenta <= 0)
            {
                return BadRequest(new { mensaje = "El precio debe ser mayor que cero" });
            }

            producto.PrecioVenta = dto.PrecioVenta;

            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Precio actualizado correctamente", id });
        }

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