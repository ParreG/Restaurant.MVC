namespace ResturantPG_MVC.ViewModel
{
    public class DishVM
    {
        public int dish_Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool IsPopular { get; set; }
        public string PictureURL { get; set; } = "";
    }

}
