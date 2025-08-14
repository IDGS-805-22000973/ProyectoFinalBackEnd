using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Dtos
{
    public class CrearComentarioDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El comentario debe tener entre 3 y 500 caracteres")]
        public string Texto { get; set; }
    }
}
