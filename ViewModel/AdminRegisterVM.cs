using System.ComponentModel.DataAnnotations;

namespace ResturantPG_MVC.ViewModel
{
    public class AdminRegisterVM
    {
        [Required, EmailAddress, MaxLength(50)]
        [Display(Name = "E-post")]
        public string Email { get; set; }

        [Required, MaxLength(30)]
        [Display(Name = "Användarnamn")]
        public string UserName { get; set; }

        [Required, MinLength(8)]
        [DataType(DataType.Password)]
        [Display(Name = "Lösenord")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Registreringskod")]
        public string RegistrationCode { get; set; }
    }
}
