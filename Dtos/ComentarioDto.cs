using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Dtos
{
    public class ComentarioDto
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioNombre { get; set; }
    }

    public class ComentarioAdminDto : ComentarioDto
    {
        public string? Respuesta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? AdminRespuestaNombre { get; set; }
    }


    public class ResponderComentarioDto
    {
        [Required]
        public int ComentarioId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Respuesta { get; set; }
    }
}
