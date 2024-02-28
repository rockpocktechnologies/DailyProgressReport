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
        private readonly List<AddEntryModel> _tempEntries = new List<AddEntryModel>(); // Temporary data structure

        public AddEntryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            //List<AddEntryModel> model = GetProjects();

            try
            {
                int projectID = 1;
                AddEntryModel model = new AddEntryModel();
                model.Projects = GetProjects();
                model.BOQHeadNames = GetBOQHeadNames(projectID);
                model.entries = GetAddEntriesFromDatabase();
                //model.all = Showdatatodatatable();

                List<AddEntryModel> projects = GetProjects();
                //List<AddEntryModel> BOQHeadName = GetBOQHeadNames(projectID);
               // List<AllAddentrydataModel> all = Showdatatodatatable();



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
                                        ProjectName = reader["ProjectName"].ToString(),
                                        BlockQuantity = reader["BlockQuantity"] != DBNull.Value ? (int)reader["BlockQuantity"] : 0,
                                        JTDQuantity = reader["JTDQuantity"] != DBNull.Value ? (int)reader["JTDQuantity"] : 0,
                                        //DayQuantity = reader["DayQuantity"] != DBNull.Value ? (int)reader["DayQuantity"] : 0,
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
        [HttpGet]
        private List<AddEntryModel> GetBOQHeadNames(int projectID)
        {
            List<AddEntryModel> BOQHeadNames = new List<AddEntryModel>();
            //return Json(BOQHeadNames.Select(x => x.BOQHeadName).ToList());


            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("select * from dpr_boqhead", connection))
                {
                   // command.CommandType = CommandType.StoredProcedure;

                    // Add the parameter for projectID
                    command.Parameters.AddWithValue("@ProjectID", projectID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AddEntryModel p = new AddEntryModel
                            {
                                ProjectID = Convert.ToInt32(reader["ProjectID"]),
                                BOQHeadName = reader["BOQHeadName"] is DBNull ? string.Empty : reader["BOQHeadName"].ToString(),
                            };

                            BOQHeadNames.Add(p);
                        }
                    }
                }
            }

            return BOQHeadNames;
        }
        
        [HttpPost]
        public ActionResult AddEntryToDatabase(string ProjectID, string BOQHeadName, DateTime Date)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    int Id;
                    using (SqlCommand retrieveCommand = new SqlCommand("SELECT Id FROM dpr_boqhead WHERE BOQHeadName = @BOQHeadName", connection))
                    {
                        retrieveCommand.Parameters.AddWithValue("@BOQHeadName", BOQHeadName);
                        Id = Convert.ToInt32(retrieveCommand.ExecuteScalar());
                    }

                    // Insert data into dpr_MaterialTransactions
                    using (SqlCommand command = new SqlCommand("INSERT INTO dpr_MaterialTransactions (ProjectID, BOQHeadID, Date) VALUES (@ProjectID, @BOQHeadID, @Date)", connection))
                    {
                        command.Parameters.AddWithValue("@ProjectID", Convert.ToInt32(ProjectID));
                        command.Parameters.AddWithValue("@BOQHeadID", Id);  // Use the retrieved BOQHeadID
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
        public IActionResult AddEntry(string projectName, string BOQHeadName, List<DynamicRowData> dynamicRows)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");
                if (loggedInUserId != null && loggedInUserId != "")
                {
                    DateTime Date = DateTime.Now;

                    AddEntryToDatabase(projectName, BOQHeadName, Date); // Replace "User123" with the actual user or logged-in user
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
        [HttpPost]
        public ActionResult SaveSubmitEntries(List<AddEntryModel> entries, bool issubmitted)
        {
            try
            {
                // Add logic to save or submit entries to the database
                SaveOrSubmitEntries(entries, issubmitted);

                return Json(new { success = true, message = issubmitted ? "Entries submitted successfully" : "Entries saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving or submitting entries", error = ex.Message });
            }
        }
        private void SaveOrSubmitEntries(List<AddEntryModel> entries, bool issubmitted)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    foreach (var entry in entries)
                    {
                        // Retrieve BOQHeadID based on BOQHeadName
                        int boqHeadId;
                        using (SqlCommand retrieveCommand = new SqlCommand("SELECT Id FROM dpr_boqhead WHERE BOQHeadName = @BOQHeadName", connection))
                        {
                            retrieveCommand.Parameters.AddWithValue("@BOQHeadName", entry.BOQHeadName);
                            boqHeadId = Convert.ToInt32(retrieveCommand.ExecuteScalar());
                        }

                        // Insert data into dpr_MaterialTransactions
                        using (SqlCommand command = new SqlCommand("INSERT INTO dpr_MaterialTransactions (ProjectID, BOQHeadID, Date,DayQuantity) VALUES (@ProjectID, @BOQHeadID, @Date,@DayQuantity)", connection))
                        {
                            command.Parameters.AddWithValue("@ProjectID", entry.ProjectID);
                            command.Parameters.AddWithValue("@BOQHeadID", boqHeadId);  // Use the retrieved BOQHeadID
                            command.Parameters.AddWithValue("@Date", entry.Date);
                            command.Parameters.AddWithValue("@DayQuantity", entry.DayQuantity);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                if (issubmitted)
                {
                    
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it
                string methodName = MethodBase.GetCurrentMethod().Name;
                string exceptionMessage = $"Exception in method '{methodName}'";
                var errorLogger = new CustomErrorLog(_configuration);
                errorLogger.LogError(ex, exceptionMessage);
                // You might want to throw the exception or handle it according to your application's needs
                // throw;
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

                    using (SqlCommand command = new SqlCommand("SELECT Id,BlockQuantity,BOQReferenceID,JTDQuantity,Date,DayQuantity FROM dpr_MaterialTransactions", connection))
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
                                    DayQuantity = reader["DayQuantity"] != DBNull.Value ? (int)reader["DayQuantity"] : 0,

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
        
        public class DynamicRowData
        {
            public string ProjectName { get; set; }
            public string BOQHeadName { get; set; }
        }
    }
}

