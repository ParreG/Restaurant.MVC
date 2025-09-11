namespace ResturantPG_MVC.DTOs.TableDTOs
{
    public class UpdatePositionsDTO
    {
        public List<TablePositionUpdate> Updates { get; set; } = new();
    }

    public class TablePositionUpdate
    {
        public int TableNumber { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
