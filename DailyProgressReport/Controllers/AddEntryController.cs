using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

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
                //model.entries = GetAddEntriesFromDatabase();
                //return View( model);
                List<AddEntryModel> projects = GetProjects();
                List<AddEntryModel> entries = GetAddEntriesFromDatabase();

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
        public IActionResult EntryList()
        {
            List<AddEntryModel> entries = GetAddEntriesFromDatabase();
            ViewBag.Entries = entries;
            return View();
        }
        private List<AddEntryModel> GetentryListFromDatabase()
        {
            List<AddEntryModel> entries = new List<AddEntryModel>();

            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null)
                {
                    CommonFunction cmn = new CommonFunction();
                    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand("dpr_sp_GetBlocksByUser", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@Username", loggedInUserId);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    AddEntryModel entry = new AddEntryModel
                                    {
                                        Id = (int)reader["Id"],
                                        //ProjectName = reader["ProjectName"].ToString(),
                                        BlockQuantity = reader["BlockQuantity"] != DBNull.Value ? (int)reader["BlockQuantity"] : 0,
                                        JTDQuantity = reader["JTDQuantity"] != DBNull.Value ? (int)reader["JTDQuantity"] : 0,
                                        Date = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["Date"].ToString()),
                                        
                                    };

                                    entries.Add(entry);
                                }
                            }
                        }
                    }
                }

            }

            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string exceptionMessage = $"Exception in method '{methodName}'";

                // Log or rethrow the exception with the updated message
                var errorLogger = new CustomErrorLog(_configuration);
                errorLogger.LogError(ex, exceptionMessage);
                //return Json(new { error = ex.Message });
            }
            return entries;

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

        [HttpPost]
        public ActionResult AddEntryToDatabase( int BlockQuantity, int BOQReferenceID, int jtdQuantity, DateTime Date)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("INSERT INTO dpr_MaterialTransactions ( BlockQuantity, BOQReferenceID, JTDQuantity,Date) VALUES ( @BlockQuantity, @BOQReferenceID, @JTDQuantity,@Date)", connection))
                    {
                        // Add parameters to the SQL command
                        command.Parameters.AddWithValue("@BlockQuantity", Convert.ToInt32 (BlockQuantity));
                        command.Parameters.AddWithValue("@BOQReferenceID", BOQReferenceID);
                        command.Parameters.AddWithValue("@JTDQuantity", jtdQuantity);
                        command.Parameters.AddWithValue("@Date", Date);


                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Entry added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding Entry", error = ex.Message });
            }
        }
            [HttpPost]
            public IActionResult AddEntry( int BlockQuantity,int BOQReferenceID, int jtdQuantity)
            {
                try
                {
                    string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");
                    if (loggedInUserId != null && loggedInUserId != "")
                    {
                    DateTime Date = DateTime.Now;

                    AddEntryToDatabase( BlockQuantity, BOQReferenceID, jtdQuantity, Date); // Replace "User123" with the actual user or logged-in user
                        return Json(new { success = true, message = "Entry added successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = $"Error adding Entry, please login and try again." });

                    }
                }
                catch (Exception ex)
                {
                    string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    string exceptionMessage = $"Exception in method '{methodName}'";

                    // Log or rethrow the exception with the updated message
                    var errorLogger = new CustomErrorLog(_configuration);
                    errorLogger.LogError(ex, exceptionMessage);
                    return Json(new { error = ex.Message });
                }
            }
        private List<AddEntryModel> GetProjectsFromDatabase()
        {
            List<AddEntryModel> projects = new List<AddEntryModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_MaterialTransactions ORDER BY Id DESC", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AddEntryModel project = new AddEntryModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ProjectName = reader["ProjectName"].ToString(),
                            };

                            projects.Add(project);
                        }
                    }
                }
            }

            return projects;
        }
        [HttpGet]
        public IActionResult GetAddEntries()
        {
            try
            {
                List<AddEntryModel> entries = GetAddEntriesFromDatabase();
                return Json(new { success = true, entries });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private List<AddEntryModel> GetAddEntriesFromDatabase()
        {
            List<AddEntryModel> entries = new List<AddEntryModel>();
            CommonFunction cmn = new CommonFunction();

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {

                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT Id,BlockQuantity,BOQReferenceID,JTDQuantity,Date FROM dpr_MaterialTransactions", connection))
                    {

                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {

                                AddEntryModel entry = new AddEntryModel
                                {

                                    Id = (int)reader["Id"],
                                    BlockQuantity = reader["BlockQuantity"] != DBNull.Value ? (int)reader["BlockQuantity"] : 0,
                                    BOQReferenceID = reader["BOQReferenceID"] != DBNull.Value ? (int)reader["BOQReferenceID"] : 0,
                                    JTDQuantity = reader["JTDQuantity"] != DBNull.Value ? (int)reader["JTDQuantity"] : 0,
                                    Date = reader["Date"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["Date"].ToString()) : "",

                                };

                                entries.Add(entry);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string exceptionMessage = $"Exception in method '{methodName}'";

                // Log or rethrow the exception with the updated message
                var errorLogger = new CustomErrorLog(_configuration);
                errorLogger.LogError(ex, exceptionMessage);
                //return Json(new { error = ex.Message });
            }

            return entries;
        }


    }
}

