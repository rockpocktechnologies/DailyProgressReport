using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.ConstrainedExecution;
using System.Configuration;

namespace DailyProgressReport.Controllers
{
    public class ActivityCodeController : Controller
    {
        private readonly IConfiguration _configuration;

        public ActivityCodeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                ShowActivity show = new ShowActivity();
                // Assuming GetProjects() returns a List<ProjectViewModel>

                List<ActivityModel> activities = GetActivity();
                List<ProjectViewModel> project = GetAssignedProjects();
                ViewBag.ProjectsData = new SelectList(project, "ProjectID", "ProjectName");
                show.Activity = activities;
                show.projects = project;


                return View(show);
            }
            catch (Exception ex)
            {
                //   LogException(ex);
                return Json(new { error = ex.Message });
            }
        }


        //private List<ProjectViewModel> GetProjectsFromDatabase()
        //{
        //    List<ProjectViewModel> projects = new List<ProjectViewModel>();

        //    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
        //    {
        //        connection.Open();

        //        using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tblProjects ORDER BY ProjectID DESC", connection))
        //        {
        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    ProjectViewModel project = new ProjectViewModel
        //                    {
        //                        ProjectID = Convert.ToInt32(reader["ProjectID"]),
        //                        ProjectName = reader["ProjectName"].ToString(),
        //                        // Add other properties as needed
        //                    };

        //                    projects.Add(project);
        //                }
        //            }
        //        }
        //    }

        //    return projects;
        //}


        private List<ProjectViewModel> GetAssignedProjects()
        {
            CommonFunction cmn = new CommonFunction();
            List<ProjectViewModel> projects = new List<ProjectViewModel>();

            // Get the logged-in user's ID (you need to implement this logic)
            //int loggedInUserId = GetLoggedInUserId();
            string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

            if (loggedInUserId != null)
            {
                // Your ADO.NET code to call the stored procedure or query to fetch assigned projects
                // Example code:
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("dpr_SPGetAssignedProjects", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@AdminId", loggedInUserId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProjectViewModel project = new ProjectViewModel
                                {
                                    ProjectID = Convert.ToInt32(reader["ProjectID"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    ProjectShortName = reader["ProjectShortName"].ToString(),
                                    ProjectCode = reader["ProjectCode"].ToString(),
                                    ProjectStartDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectStartDate"].ToString()),
                                    ProjectEndDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectEndDate"].ToString()),
                                    ProjectRevisedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectRevisedDate"].ToString()),
                                    IsActive = reader["IsActive"] == DBNull.Value ? true : Convert.ToBoolean(reader["IsActive"])
                                };

                                projects.Add(project);
                            }
                        }
                    }
                }
            }

            return projects;
        }





        [HttpPost]
        public ActionResult AddActivityCode(int projectName, string ActivityCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("dpr_sp_AddActivityCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters to the stored procedure
                        command.Parameters.AddWithValue("@ProjectID ", Convert.ToInt32(projectName));

                        command.Parameters.AddWithValue("@ActivityCode", ActivityCode);

                        command.ExecuteNonQuery();
                    }
                }


                return Json(new { success = true, message = "Activity Code added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding Activity Code", error = ex.Message });
            }
        }




        [HttpGet]
        public IActionResult GetActivityById(int id)
        {
            try
            {
                ActivityModel activity = GetActivityByIdFromDatabase(id);
                return Json(new { success = true, ActivityModel = activity });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpPost]
        public IActionResult UpdateActivity(int id, string projectName, string activityCode)
        {
            try
            {

                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");
                // Update the activity code and project name in the database
                UpdateActivityInDatabase(id, projectName, activityCode);
                return Json(new { success = true, message = "Activity Code updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpPost]
        public ActionResult DeleteActivityCode(int Id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    string sqlQuery = "DELETE FROM dpr_tbl_ActivityCode WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Activity Code deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting Group Code", error = ex.Message });
            }
        }



        private ActivityModel GetActivityByIdFromDatabase(int id)
        {
            ActivityModel Activity = null;
            CommonFunction cmn = new CommonFunction();
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();


                    using (SqlCommand command = new SqlCommand("dpr_sp_GetActivityCodeById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Id", id);


                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Activity = new ActivityModel
                                {
                                    Id = (int)reader["Id"],
                                    // ProjectID = Convert.ToInt32(reader["ProjectID"]),

                                    ProjectName = reader["ProjectName"].ToString(),
                                    ActivityCode = reader["ActivityCode"].ToString(),

                                };
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {


                throw;
            }


            return Activity;
        }




        private List<ActivityModel> GetActivity()    //  Get Activity Code Prop
        {
            CommonFunction cmn = new CommonFunction();
            List<ActivityModel> projects = new List<ActivityModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                string sqlQuery = "SELECT a.id, p.ProjectName, a.ActivityCode " +
                                  "FROM dpr_tblProjects AS p " +
                                  "JOIN dpr_tbl_ActivityCode AS a ON p.ProjectID = a.ProjectID " +
                                  "ORDER BY a.Id DESC";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ActivityModel project = new ActivityModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                ActivityCode = reader["ActivityCode"].ToString()
                            };

                            projects.Add(project);
                        }
                    }
                }
            }

            return projects;
        }




        [HttpPost]
        public ActionResult UpdateActivityInDatabase(int id, string projectName, string activityCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("dpr_sp_UpdateActivityCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters to the stored procedure
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@ProjectID", Convert.ToInt32(projectName));
                        command.Parameters.AddWithValue("@ActivityCode", activityCode);

                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Activity Code updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating Activity Code", error = ex.Message });
            }
        }




    }
}