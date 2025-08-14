namespace ProyectoFinal.Dtos
{
    public class ClienteDTO
    {
        public class ActualizarNombreDto
        {
            public string Nombre { get; set; }
        }

        public class ActualizarEmailDto
        {
            public string NuevoEmail { get; set; }
        }

        public class CambiarPasswordDto
        {
            public string PasswordActual { get; set; }
            public string NuevoPassword { get; set; }
        }
    }
}
