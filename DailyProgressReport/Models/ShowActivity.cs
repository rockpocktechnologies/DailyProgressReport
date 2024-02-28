namespace DailyProgressReport.Models
{
    public class ShowActivity
    {
        public List<ProjectViewModel> projects { get; set; }

        public List<ActivityModel> Activity { get; set; }
        public List<GroupCodeModel> Group { get; set; }
    }
}
