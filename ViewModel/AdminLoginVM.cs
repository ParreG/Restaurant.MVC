using System.ComponentModel.DataAnnotations;

namespace ResturantPG_MVC.ViewModel
{
    public class AdminLoginVM
    {
        [Required]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }


    public class LoginResponseVM
    {
        public string Token { get; set; }
    }
}
