using DailyProgressReport.Models;
using System.ComponentModel.DataAnnotations;

namespace DailyProgressReport
{
    public class ProjectViewModel
    {
        public List<ProjectType> ProjectType { get; set; }

        public int ProjectID { get; set; }
        public int ID { get; set; }


        public string Value { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ProjectShortName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ProjectCode { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ProjectStartDate { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ProjectEndDate { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ProjectRevisedDate { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string IsProjectCodeUnique { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public bool ProjectCodeUniqueValidation { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsLocationRequired { get; set; }


        public bool IsActive { get; set; }
        
    
}

}
