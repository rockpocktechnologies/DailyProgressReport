using System.ComponentModel.DataAnnotations;

namespace DailyProgressReport.Models
{
    public class SaveProjectModel
    {
        public int? ProjectID { get; set; }

        [Required(ErrorMessage = "Project Name is required")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Short Name is required")]
        public string ProjectShortName { get; set; }

        [Required(ErrorMessage = "Project Code is required")]
        public string ProjectCode { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public string ProjectStartDate { get; set; }
        public string ProjectEndDate { get; set; }

        public string ProjectRevisedDate { get; set; }
         public string IsLocationRequired { get; set; }

        
    }

}
