namespace DailyProgressReport.Models
{
    public class DprBOQReference
    {
        public int Id { get; set; }
        public string BOQHead { get; set; }
        public string BOQNo { get; set; }
        public string WBSNumber { get; set; }
        public string BOQDescription { get; set; }
        public string UOM { get; set; }
        public string Length { get; set; }
        public string GroupCode { get; set; }
        public string ActivityCode { get; set; }
        public string DesignQuantity { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}