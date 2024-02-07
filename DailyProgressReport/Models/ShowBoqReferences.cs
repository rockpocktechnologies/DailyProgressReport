namespace DailyProgressReport.Models
{

    public class ShowBoqReferences
    {
        public List<DprBOQHead> BoqHeads { get; set; }
        public List<DprBOQReference> BoqReferences { get; set; }
        public List<PlanningManagerModel> Planning { get; set; }
        public List<ProjectViewModel> Project { get; set; }
        public List<GroupCodeModel> Group { get; set; }


        public List<ProjectViewModel> project { get; set; }

        public List<DprComponent> Component { get; set; }

        public List<ActivityModel> Activity { get; set; }
    }

}
