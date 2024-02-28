using System.Globalization;

namespace DailyProgressReport.Classes
{
    public class CommonFunction
    {
        public string ConvertDateFormatddmmyytommddyyDuringSave(string inputDate)
        {
            if (inputDate != null)
            {
                // Split the input date by "/", assuming the format could be dd/mm/yy or mm/dd/yy
                string[] dateParts = inputDate.Split('/');
                if (dateParts.Length == 3)
                {
                    // Try parsing in dd/mm/yy format
                    if (int.TryParse(dateParts[0], out int day) &&
                        int.TryParse(dateParts[1], out int month) &&
                        int.TryParse(dateParts[2], out int year))
                    {
                        // Create a DateTime instance with the parsed values
                        DateTime date = new DateTime(year, month, day);

                        // Format the date as mm/dd/yy
                        string convertedDate = date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

                        return convertedDate;
                    }
                }
            }
            // If the input format is not recognized, handle the error or return the input date as-is

            return "";
        }

        public string ConvertDateFormatmmddyytoddmmyyDuringDisplay(string inputDate)
        {

            if (inputDate != null)
            {
                string[] dateFormats = new string[]
               {
                "MMM dd yyyy hh:mmtt",   // Format 1
                "MMM  dd yyyy hh:mmtt",   // Format 1
                "MMMM dd yyyy hh:mmtt",  // Format 2
                "MMMM  dd yyyy hh:mmtt",  // Format 2
                "dd/MM/yy",              // Format 3 (desired output format)
                "dd-MM-yyyy HH:mm:ss",   // Format 4 (if needed)
                "MMM d yyyy hh:mmtt",     // Custom Format for "Jul 5 2024 12:00AM"
                "MMM  d yyyy hh:mmtt",
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "MM/dd/yyyy",
                "MMM dd yyyy hh:mmtt",   // Format 1
        "MMM  dd yyyy hh:mmtt",  // Format 1
         "MM/dd/yyyy hh:mm:ss tt",
         "dd MMM yyyy",
         "yyyy-MM-dd",
         "dd-MMM-yy hh:mm:ss tt",
         "yyyy-MM-dd HH:mm:ss"

                   // Add more date formats here as needed
               };
                if (inputDate.Trim() == "Jan 1 1900 12:00AM" || inputDate.Trim() == "Jan  1 1900 12:00AM"
                   || inputDate.Trim() == "01-01-1900 00:00:00" || inputDate.Trim() == "")
                {
                    // Handle null date or the default date as needed
                    return ""; // Or any other representation you prefer for null/default dates
                }
                else
                {
                    DateTime date = DateTime.ParseExact(inputDate, dateFormats, CultureInfo.InvariantCulture);

                    // Format the date as dd/mm/yy
                    string convertedDate = date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                    return convertedDate;
                }
            }
            else
            {
                return "";
            }
            //if (DateTime.TryParseExact(inputDate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            //    {
            //        // Format the date as dd/mm/yy
            //        string convertedDate = parsedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            //        return convertedDate;
            //    }
            //    else
            //    {
            //        // Handle the case where the date cannot be parsed
            //        return "Invalid Date";
            //    }
            //}
            //else
            //{
            //    return "";
            //}
        }
        
    }
}
