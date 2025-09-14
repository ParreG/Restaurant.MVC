using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResturantPG_MVC.Extensions;
using ResturantPG_MVC.ViewModel;

namespace ResturantPG_MVC.Controllers
{
    public class AdminBookingsController : Controller
    {
        private readonly HttpClient httpClient;

        public AdminBookingsController(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient("ResturangApi");
        }


        [HttpGet("GetAllBookings")]
        [Authorize]
        public async Task<IActionResult> AdminBookings()
        {
            httpClient.AddAuthHeader(HttpContext);

                var response = await httpClient.GetAsync("Booking/GetAllBookings");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte hämta bokningar.";
                    return View(new List<BookingVM>());
                }

                var bookings = await response.Content.ReadFromJsonAsync<List<BookingVM>>();
                return View(bookings ?? new List<BookingVM>());
            
        }


        [HttpGet("AdminCreateBooking")]
        [Authorize]
        public IActionResult AdminCreateBooking()
        {
            var vm = new NewBookingVM
            {
                BookingStart = DateTime.Now
            };
            return View("AdminCreateBooking", vm);
        }


        [HttpPost("AdminCreateBooking")]
        [Authorize]
        public async Task<IActionResult> AdminCreateBooking(NewBookingVM newbooking)
        {
            if (!ModelState.IsValid)
            {
                return View(newbooking);
            }

            httpClient.AddAuthHeader(HttpContext);

            var response = await httpClient.PostAsJsonAsync("Booking/AddNewBooking", new
            {
                newbooking.Name,
                newbooking.LastName,
                newbooking.Email,
                newbooking.Tel,
                newbooking.BookingStart,
                newbooking.NumberOfGuests
            });

            if (!response.IsSuccessStatusCode)
            {
                return View(newbooking);
            }

            return RedirectToAction(nameof(AdminBookings));
        }



        [HttpGet("UpdateBooking/{id}")]
        [Authorize]
        public async Task<IActionResult> AdminUpdateBooking(int id)
        {
            httpClient.AddAuthHeader(HttpContext);

            var response = await httpClient.GetAsync($"Booking/GetBookingById/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var booking = await response.Content.ReadFromJsonAsync<BookingVM>();
            if (booking == null || booking.Guest == null)
            {
                return NotFound();
            }

            var updateBooking = new UpdateBookingVM
            {
                BookingStart = booking.BookingStart,
                NumberOfGuests = booking.NumberOfGuests,
                Email = booking.Guest.Email
            };

            return View(updateBooking);
        }


        // POST: /AdminBookings/UpdateBooking/5
        [HttpPost("UpdateBooking/{id}")]
        [Authorize]
        public async Task<IActionResult> AdminUpdateBooking(int id, UpdateBookingVM updateBooking)
        {
            httpClient.AddAuthHeader(HttpContext);

            if (!ModelState.IsValid)
            {
                return View(updateBooking);
            }

            var response = await httpClient.PutAsJsonAsync($"Booking/UpdateBooking/{id}", updateBooking);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(AdminBookings));
            }

            ModelState.AddModelError("", "Uppdateringen misslyckades.");
            return View(updateBooking);
        }



        [HttpGet("DeleteBooking/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            httpClient.AddAuthHeader(HttpContext);

            var response = await httpClient.DeleteAsync($"Booking/DeleteBooking/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(AdminBookings));
            }

            return RedirectToAction(nameof(AdminBookings));
        }
    }
}
