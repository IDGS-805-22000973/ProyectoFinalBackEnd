using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Dtos;
using ProyectoFinal.Models;
using System.Security.Claims;
// 1. AÑADE EL USING PARA EL SERVICIO DE CORREO
using ProyectoFinal.Services;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // Requiere autenticación para todos los endpoints
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService; // 2. DECLARA LA VARIABLE PARA EL SERVICIO DE CORREO

        // 3. INYECTA EL SERVICIO EN EL CONSTRUCTOR
        public VentasController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearVenta([FromBody] CrearVentaAdminDto dto)
        {
            try
            {
                // Validar el producto con sus componentes
                var producto = await _context.Productos
                    .Include(p => p.Componentes)
                    .ThenInclude(c => c.MateriaPrima)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProductoId);

                if (producto == null)
                {
                    return NotFound(new { mensaje = $"Producto con ID {dto.ProductoId} no encontrado" });
                }

                // Validar el usuario cliente
                var cliente = await _userManager.FindByIdAsync(dto.ClienteId);
                if (cliente == null)
                {
                    return NotFound(new { mensaje = $"Cliente con ID {dto.ClienteId} no encontrado" });
                }

                // Verificar que el usuario sea cliente
                var clienteRoles = await _userManager.GetRolesAsync(cliente);
                if (!clienteRoles.Contains("Cliente"))
                {
                    return BadRequest(new { mensaje = "El usuario especificado no tiene rol de Cliente" });
                }

                // Verificar existencias antes de procesar la venta
                foreach (var componente in producto.Componentes)
                {
                    var materiaNecesaria = componente.Cantidad * dto.Cantidad;
                    if (componente.MateriaPrima.Existencia < materiaNecesaria)
                    {
                        return BadRequest(new
                        {
                            mensaje = $"No hay suficiente existencia de {componente.MateriaPrima.Nombre}",
                            materiaPrima = componente.MateriaPrima.Nombre,
                            existenciaActual = componente.MateriaPrima.Existencia,
                            cantidadNecesaria = materiaNecesaria
                        });
                    }
                }


                foreach (var componente in producto.Componentes)
                {
                    var materiaPrima = await _context.MateriaPrima.FindAsync(componente.MateriaPrimaId);
                    materiaPrima.Existencia -= componente.Cantidad * dto.Cantidad;
                    _context.MateriaPrima.Update(materiaPrima);
                }

                // Crear la venta
                var venta = new Venta
                {
                    FechaVenta = DateTime.UtcNow,
                    Cantidad = dto.Cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    Total = producto.PrecioVenta * dto.Cantidad,
                    ProductoId = dto.ProductoId,
                    UsuarioId = dto.ClienteId,
                    Estado = "Completada",
                    CreadoPorAdminId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // 4. LLAMA AL SERVICIO DE CORREO DESPUÉS DE GUARDAR LA VENTA
                await _emailService.EnviarConfirmacionVentaAsync(venta, cliente, producto);

                return Ok(new
                {
                    mensaje = "Venta creada y notificada correctamente",
                    venta.Id,
                    venta.Total,
                    Producto = producto.Nombre,
                    Cliente = cliente.Nombre,
                    MateriasPrimasUtilizadas = producto.Componentes.Select(c => new {
                        Nombre = c.MateriaPrima.Nombre,
                        CantidadUtilizada = c.Cantidad * dto.Cantidad,
                        NuevaExistencia = c.MateriaPrima.Existencia
                    }).ToList()
                });
            }
            catch (DbUpdateException ex)
            {
                // Loggear el error completo
                Console.WriteLine($"Error al crear venta: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, new
                {
                    mensaje = "Error al crear la venta",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("mis-ventas")]
        //[Authorize(Roles = "Cliente")] 
        public async Task<IActionResult> ObtenerMisVentas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ventas = await _context.Ventas
                .Where(v => v.UsuarioId == userId)
                .Include(v => v.Producto)
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
                        Nombre = v.CreadoPorAdmin.Nombre
                    } : null
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("cliente/{clienteId}")]
        //[Authorize(Roles = "Admin")] 
        public async Task<IActionResult> ObtenerVentasPorCliente(string clienteId)
        {
            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                return NotFound(new { mensaje = $"Cliente con ID {clienteId} no encontrado" });
            }

            var ventas = await _context.Ventas
                .Where(v => v.UsuarioId == clienteId)
                .Include(v => v.Producto)
                .Include(v => v.CreadoPorAdmin)
                .Select(v => new VentaAdminDto
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
                    CreadoPor = v.CreadoPorAdmin != null ? v.CreadoPorAdmin.Nombre : "Sistema"
                })
                .ToListAsync();

            return Ok(new
            {
                Cliente = new { Id = cliente.Id, Nombre = cliente.Nombre },
                Ventas = ventas
            });
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")] 
        public async Task<IActionResult> ObtenerTodasLasVentas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Producto)
                .Include(v => v.Usuario)
                .Include(v => v.CreadoPorAdmin)
                .Select(v => new VentaAdminDto
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
                    Cliente = new UsuarioDto
                    {
                        Id = v.Usuario.Id,
                        Nombre = v.Usuario.Nombre,
                        Email = v.Usuario.Email
                    },
                    CreadoPor = v.CreadoPorAdmin != null ? v.CreadoPorAdmin.Nombre : "Sistema"
                })
                .ToListAsync();

            return Ok(ventas);
        }



        
        [HttpGet("resumen")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetResumenVentas()
        {
            var hoy = DateTime.UtcNow;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var resumen = new
            {
                TotalVentasMes = await _context.Ventas
                    .Where(v => v.FechaVenta >= inicioMes && v.FechaVenta <= hoy)
                    .SumAsync(v => v.Total),
                TotalVentasAnio = await _context.Ventas
                    .Where(v => v.FechaVenta.Year == hoy.Year)
                    .SumAsync(v => v.Total),
                VentasHoy = await _context.Ventas
                    .Where(v => v.FechaVenta.Date == hoy.Date)
                    .CountAsync(),
                PromedioVenta = await _context.Ventas
                    .Where(v => v.FechaVenta.Year == hoy.Year)
                    .AverageAsync(v => v.Total)
            };

            return Ok(resumen);
        }

        [HttpGet("recientes")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVentasRecientes()
        {
            var ventas = await _context.Ventas
                .OrderByDescending(v => v.FechaVenta)
                .Take(5)
                .Include(v => v.Producto)
                .Include(v => v.Usuario)
                .Select(v => new
                {
                    v.Id,
                    v.FechaVenta,
                    Producto = v.Producto.Nombre,
                    Cliente = v.Usuario.Nombre,
                    v.Total,
                    v.Estado
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpGet("por-mes")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVentasPorMes()
        {
            var hoy = DateTime.UtcNow;
            var inicioAnio = new DateTime(hoy.Year, 1, 1);

            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioAnio && v.FechaVenta <= hoy)
                .Select(v => new { v.FechaVenta, v.Total })
                .ToListAsync();

            var ventasPorMes = ventas
                .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                .Select(g => new
                {
                    Mes = $"{g.Key.Year}-{g.Key.Month.ToString().PadLeft(2, '0')}",
                    Total = g.Sum(v => v.Total),
                    Cantidad = g.Count()
                })
                .OrderBy(g => g.Mes)
                .ToList();

            return Ok(ventasPorMes);
        }

        [HttpGet("top-clientes")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopClientes()
        {
            var topClientes = await _context.Ventas
                .GroupBy(v => new { v.Usuario.Id, v.Usuario.Nombre })
                .Select(g => new
                {
                    ClienteId = g.Key.Id,
                    ClienteNombre = g.Key.Nombre,
                    TotalCompras = g.Sum(v => v.Total),
                    CantidadCompras = g.Count()
                })
                .OrderByDescending(g => g.TotalCompras)
                .Take(5)
                .ToListAsync();

            return Ok(topClientes);
        }

        [HttpGet("top-productos")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopProductos()
        {
            var topProductos = await _context.Ventas
                .GroupBy(v => new { v.Producto.Id, v.Producto.Nombre })
                .Select(g => new
                {
                    ProductoId = g.Key.Id,
                    ProductoNombre = g.Key.Nombre,
                    TotalVentas = g.Sum(v => v.Total),
                    CantidadVendida = g.Sum(v => v.Cantidad)
                })
                .OrderByDescending(g => g.TotalVentas)
                .Take(5)
                .ToListAsync();

            return Ok(topProductos);
        }
    }
}