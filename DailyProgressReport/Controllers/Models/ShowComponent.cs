namespace DailyProgressReport.Models
{
    public class ShowComponent
    {
        public List<ProjectViewModel> projects { get; set; }

        public List<DprComponent> compo { get; set; }
        public List<DprComponent> ProjecName { get; set; }  
        public List<ActivityModel> Activity { get; set; }
        public List<GroupCodeModel> Group { get; set; }
    }
}
