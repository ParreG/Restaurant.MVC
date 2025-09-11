namespace ResturantPG_MVC.Models
{
    public class Dish
    {
        public int Dish_Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public bool IsPopular { get; set; }

        public string PictureUrl { get; set; }

    }
}
