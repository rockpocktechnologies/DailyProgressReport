namespace DailyProgressReport.Models
{
    public class DprVillageModel
    {
        public int Id { get; set; }
        public string VillageName { get; set; }        
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string BlockName { get; set; }
        public int ComponentId { get; set; }
        public int BlockId { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
