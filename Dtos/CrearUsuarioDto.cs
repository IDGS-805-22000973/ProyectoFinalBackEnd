namespace ProyectoFinal.Dtos
{
    public class CrearUsuarioDto
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; } // "Admin" o "Cliente"
    }
}
