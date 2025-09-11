using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResturantPG_MVC.ViewModel
{
    public class CreateDishVM
    {
        [Display(Name = "Maträttens Namn")]
        public string Name { get; set; }

        [Display(Name = "Maträttens beskrivning")]
        public string Description { get; set; }

        [Display(Name = "Maträttens pris")]
        public decimal Price { get; set; }

        public bool IsPopular { get; set; }

        [Display(Name = "Är populär?")]
        public string? PictureUrl { get; set; }
    }
}
