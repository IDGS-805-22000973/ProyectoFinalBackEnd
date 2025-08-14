using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    // En ProyectoFinal.Models
    public class Comentario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public AppUser Usuario { get; set; }

        [Required]
        [MaxLength(500)]
        public string Texto { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Nuevos campos para respuestas
        public string? Respuesta { get; set; }
        public string? AdminRespuestaId { get; set; }
        public DateTime? FechaRespuesta { get; set; }

        [ForeignKey("AdminRespuestaId")]
        public AppUser? AdminRespuesta { get; set; }
    }
}
