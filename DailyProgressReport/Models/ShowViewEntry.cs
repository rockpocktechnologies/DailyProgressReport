namespace DailyProgressReport.Models
{
    public class ShowViewEntry
    {
        public List<ProjectViewModel> projects { get; set; }

        // public List<ActivityModel> Activity { get; set; }
        // public List<GroupCodeModel> Group { get; set; }
        public List<ViewEntryViewModel> viewentry { get; set; }


        public List<ErrorViewModel> ErrorViewModel { get; set; }
    }
}
