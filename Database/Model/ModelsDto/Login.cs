
using System.ComponentModel.DataAnnotations;


namespace Database.Model.Apimodels
{
    public class Login
    {
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; }
    }
}
