using System.ComponentModel.DataAnnotations;

namespace ResturantPG_MVC.ViewModel
{
    public class UpdateBookingVM
    {
        [Required(ErrorMessage = "Starttid krävs")]
        public DateTime BookingStart { get; set; }

        [Range(1, 8, ErrorMessage = "Antal gäster måste vara 1–8")]
        public int NumberOfGuests { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
