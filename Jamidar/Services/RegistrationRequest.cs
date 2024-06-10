using System.ComponentModel.DataAnnotations;

namespace Jamidar.Services
{
    public class RegistrationRequest
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        public IList<string> Role { get; set; } = [];
    }
}
