namespace ResturantPG_MVC.ViewModel
{
    public class CreateBookingVM
    {
        public string Name { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Tel { get; set; } = "";
        public int NumberOfGuests { get; set; }
        public DateTime BookingStart { get; set; }
    }
}