namespace DailyProgressReport.Models
{
    public class AddEntryModel
    {

        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public List<AddEntryModel> Projects { get; set; }
        public int BlockQuantity { get; set; }
        public int JTDQuantity { get; set; }
        public int DayQuantity { get; set; }

        
        public string Date { get; set; }
        public int BOQReferenceID { get; set; }
        public List<AddEntryModel> entries { get; set; }
        public string BOQHeadName { get; set; }
        public List<AddEntryModel> BOQHeadNames { get; set; }
        public int BOQHeadID { get; set; }
        public string BOQHead { get; set; }

        






    }
}
