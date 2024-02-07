using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DailyProgressReport
{
    public class PlanningManagerModel
    {
        public List<string> SiteEngineerIds { get; set; }
        public int ProjectId { get; set; }
        public string SiteEngineerId { get; set; }
        public string Projects { get; set; }
        public string SiteEngineerName { get; set; }
        public string AssignmentDate { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectShortName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectStartDate { get; set; }
        public string ProjectEndDate { get; set; }

        public string ProjectRevisedDate { get; set; }

        public bool IsActive { get; set; }
        public List<SelectListItem> projectTypes { get; set; }





    }

}