namespace DailyProgressReport.Models
{
    public class AllAddentrydataModel
    {
        public string Date { get; set; }
       

        public int Id { get; set; }

        public bool issubmitted { get; set; }
        public string BOQHeadID { get; set; }
        public string BOQReferenceID { get; set; }
        public string BlockQuantity { get; set; }
        public string JTDQuantity { get; set; }
        public string DayQuantity { get; set; }
        public string ProjectID { get; set; }
        public List<AddEntryModel> Projects { get; set; }
        public List<AddEntryModel> BOQHeadNames { get; set; }


    }
}
