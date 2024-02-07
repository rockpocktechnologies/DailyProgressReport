
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using DailyProgressReport.Models;

namespace DailyProgressReport.Controllers
{
    public class ErrorLogController : Controller
    {
        private readonly string connectionString;

        public ErrorLogController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("ConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string not found in appsettings.json");
            }
        }

        public IActionResult Index(int topCount = 500)
        {
            // Fetch error logs based on the specified top count
            var errorLogs = GetTopErrorLogs(topCount);

            return View(errorLogs);
        }

        private List<ErrorLogViewModel> GetTopErrorLogs(int topCount)
        {
            List<ErrorLogViewModel> errorLogs = new List<ErrorLogViewModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPGetTopErrorLogs", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@TopCount", topCount);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var errorLog = new ErrorLogViewModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                LogTime = Convert.ToDateTime(reader["LogTime"]),
                                ErrorMessage = reader["ErrorMessage"].ToString(),
                                StackTrace = reader["StackTrace"].ToString(),
                                AdditionalInfo = reader["AdditionalInfo"].ToString()
                            };

                            errorLogs.Add(errorLog);
                        }
                    }
                }
            }

            return errorLogs;
        }


    }

}
