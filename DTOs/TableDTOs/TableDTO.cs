namespace ResturantPG_MVC.DTOs.TableDTOs
{
    public class TableDTO
    {
        public int Table_Id { get; set; }
        public int Number { get; set; }
        public int Capacity { get; set; }
        public int? X { get; set; }    
        public int? Y { get; set; }     
        public bool IsAvailable { get; set; } = true;
    }
}
