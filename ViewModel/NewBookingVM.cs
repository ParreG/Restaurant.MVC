using System.ComponentModel.DataAnnotations;

namespace ResturantPG_MVC.ViewModel
{
    public class NewBookingVM
    {
        [Required] public string Name { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Tel { get; set; } = "";

        [Required(ErrorMessage = "Starttid krävs")]
        public DateTime BookingStart { get; set; } = DateTime.Now;

        [Range(1, 8, ErrorMessage = "Max 8 gäster!")]
        public int NumberOfGuests { get; set; }
    }
}
