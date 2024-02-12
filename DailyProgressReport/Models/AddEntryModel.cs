namespace DailyProgressReport.Models
{
    public class AddEntryModel
    {
        public List<ProjectViewModel> projects { get; set; }

        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public List<AddEntryModel> Projects { get; set; }


    }
}
