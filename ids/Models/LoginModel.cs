using System.ComponentModel.DataAnnotations;

namespace ids.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
       // public string Button { get; set; }
    }
}
