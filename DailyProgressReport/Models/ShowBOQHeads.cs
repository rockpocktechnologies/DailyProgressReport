namespace DailyProgressReport.Models
{
    public class ShowBOQHeads
    {
        public List<ProjectViewModel> projects { get; set; }

        public List<DprBOQHead> boq { get; set; }
    }
}
