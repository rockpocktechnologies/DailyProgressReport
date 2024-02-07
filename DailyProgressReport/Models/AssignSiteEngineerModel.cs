using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DailyProgressReport
{
    public class AssignSiteEngineerModel
    {
        public int ProjectId { get; set; }
        public List<string> SiteEngineerIds { get; set; }
        public List<SelectListItem> Projects { get; set; }
        public List<string> SiteEngineerNames { get; set; }
    }

}

