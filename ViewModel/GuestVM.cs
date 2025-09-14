using System.ComponentModel.DataAnnotations;

namespace ResturantPG_MVC.ViewModel
{
    public class GuestVM
    {
        public int Guest_Id { get; set; }
        public string Name { get; set; }

        public string LastName { get; set; }

        public string Tel { get; set; }

        public string Email { get; set; }
    }
}
