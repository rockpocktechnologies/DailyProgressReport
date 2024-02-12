using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace DailyProgressReport.Controllers
{
    public class AddEntryController : Controller
    {
        private readonly IConfiguration _configuration;

        public AddEntryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            //List<AddEntryModel> model = GetProjects();

            try
            {
                AddEntryModel model = new AddEntryModel();
                model.Projects = GetProjects();
                //return View( model);
                List<AddEntryModel> projects = GetProjects();
                return View(model);
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string exceptionMessage = $"Exception in method '{methodName}'";

                var errorLogger = new CustomErrorLog(_configuration);
                errorLogger.LogError(ex, exceptionMessage);
                return Json(new { error = ex.Message });
            }

        }
        private List<AddEntryModel> GetProjects()
        {
            CommonFunction cmn = new CommonFunction();
            List<AddEntryModel> projects = new List<AddEntryModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tblProjects", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AddEntryModel p = new AddEntryModel
                            {
                                ProjectID = Convert.ToInt32(reader["ProjectID"]),
                                ProjectName = reader["ProjectName"] is DBNull ? string.Empty : reader["ProjectName"].ToString(),

                                //ProjectName = reader["ProjectName"].ToString(),
                                
                               
                            };

                            projects.Add(p);
                        }
                    }
                }
            }

            return projects;
        }
    }
}
