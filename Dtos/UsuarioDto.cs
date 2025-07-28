namespace ProyectoFinal.Dtos
{
    public class UsuarioDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}