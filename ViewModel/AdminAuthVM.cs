namespace ResturantPG_MVC.ViewModel
{
    public class AdminAuthVM
    {
        public AdminLoginVM Login { get; set; } = new();
        public AdminRegisterVM Register { get; set; } = new();

        public bool ShowRegister { get; set; } = false;
    }
}
