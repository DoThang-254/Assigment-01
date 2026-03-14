using System.ComponentModel.DataAnnotations;

namespace Presentation.Controllers
{
    public class LogoutRequest
    {
        [Required(ErrorMessage = "Refresh Token is required")]
        public string RefreshToken { get; set; }
    }
}