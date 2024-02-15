namespace DailyProgressReport.Models
{
    public class ShowPlanningModel
    {
        public List<ProjectViewModel> projects { get; set; }
        public List<PlanningManagerModel> Planning { get; set; }
        public List<string> SiteEngineerIds { get; set; }
        public List<string> ProjectType { get; set; }

    }
}
