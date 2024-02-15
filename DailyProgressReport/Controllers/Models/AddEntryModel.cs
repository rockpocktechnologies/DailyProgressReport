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
        public string Date { get; set; }
        public int BOQReferenceID { get; set; }

        

    }
}
