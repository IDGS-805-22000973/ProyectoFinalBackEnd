namespace ProyectoFinal.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiracion { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public string Mensaje { get; set; }
    }
}
