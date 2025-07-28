using Microsoft.AspNetCore.Identity;

namespace ProyectoFinal.Models
{
    public class AppUser : IdentityUser
    {
        public string Nombre { get; set; }
    }
}
