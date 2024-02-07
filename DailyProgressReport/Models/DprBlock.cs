namespace DailyProgressReport.Models
{
    public class DprBlock
    {
        public int Id { get; set; }
        public string BlockName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
