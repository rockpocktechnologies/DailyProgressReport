namespace DailyProgressReport.Models
{
    public class ErrorLogViewModel
    {
        public int Id { get; set; }
        public DateTime LogTime { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
