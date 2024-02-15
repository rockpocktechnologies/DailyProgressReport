namespace DailyProgressReport.Models
{
    public class DprComponent
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }

        public string ComponentName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
    }
}
