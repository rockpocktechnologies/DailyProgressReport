namespace DailyProgressReport.Models
{
    public class ViewEntryViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }

        public DateTime? Date { get; set; }
        public string? JobCodeID { get; set; }
        public string? BlockName { get; set; }
        public string? ComponentName { get; set; }
        public string? LocationName { get; set; }
        public string? VillageName { get; set; }
        public string BOQHeadName { get; set; }
        public int BOQReferenceID { get; set; }
        public string? ActivityCode { get; set; }
        public int? BlockQuantity { get; set; }
        public string TypeOfPipe { get; set; }
        public string DiaOfPipe { get; set; }
        public string UOM { get; set; }
        public int? JTDQuantity { get; set; }
        public int? DayQuantity { get; set; }

        public bool? IsSubmitted { get; set; }
        public string WBSNumber { get; set; }
    }

}
