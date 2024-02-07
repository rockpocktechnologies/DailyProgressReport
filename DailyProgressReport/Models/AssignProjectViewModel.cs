using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace DailyProgressReport
{
    public class AssignProjectViewModel
    {
        public int ProjectId { get; set; }
        public List<string> adminIds { get; set; }
        public List<SelectListItem> Projects { get; set; }
        public List<string> AdminNames { get; set; }
    }

}

