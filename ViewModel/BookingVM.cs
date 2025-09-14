using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResturantPG_MVC.ViewModel
{
    public class BookingVM
    {
        public int Booking_Id { get; set; }
        public DateTime BookingStart { get; set; }
        public DateTime? BookingEnd { get; set; }
        public int NumberOfGuests { get; set; }

        public int TableId_Fk { get; set; }
        public int GuestId_Fk { get; set; }

        public TableVM? Table { get; set; }
        public GuestVM? Guest { get; set; }
    }
}
