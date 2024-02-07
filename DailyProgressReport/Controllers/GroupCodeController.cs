using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Policy;

namespace DailyProgressReport.Controllers
{
    public class GroupCodeController : Controller
    {


        private readonly IConfiguration _configuration; // Inject your configuration if needed

        public GroupCodeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            try
            {

                ShowGroupCode show = new ShowGroupCode();
                // Assuming GetProjects() returns a List<ProjectViewModel>
                List<GroupCodeModel> groups = GetGroup();
                List<ProjectViewModel> project = GetAssignedProjects();
                ViewBag.ProjectsData = new SelectList(project, "ProjectID", "ProjectName");
                show.Group = groups;
                show.projects = project;

                return View(show);
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
        public ActionResult AddGroupCode(int projectName, string GroupCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("dpr_sp_AddGroupCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters to the stored procedure
                        command.Parameters.AddWithValue("@ProjectID ", Convert.ToInt32(projectName));
                        command.Parameters.AddWithValue("@GroupCode", GroupCode);

                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Group Code added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding Group Code", error = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetGroupById(int id)
        {
            try
            {
                GroupCodeModel boqReference = GetGroupByIdFromDatabase(id);
                return Json(new { success = true, boqReference });
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

        public ActionResult DeleteGroupCode(int Id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    string sqlQuery = "DELETE FROM dpr_tbl_GroupCode WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Group Code deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting Group Code", error = ex.Message });
            }
        }





        private List<GroupCodeModel> GetGroup()      // Get List Group Code 
        {
            CommonFunction cmn = new CommonFunction();
            List<GroupCodeModel> projects = new List<GroupCodeModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetGroupCode", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GroupCodeModel project = new GroupCodeModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                GroupCode = reader["GroupCode"].ToString(),

                            };

                            projects.Add(project);
                        }
                    }
                }
            }

            return projects;
        }

        private GroupCodeModel GetGroupByIdFromDatabase(int id)
        {
            GroupCodeModel Activity = null;
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
                                Activity = new GroupCodeModel
                                {
                                    Id = (int)reader["Id"],
                                    //  ProjectName = reader["ProjectName"].ToString(),
                                    GroupCode = reader["GroupCode"].ToString(),

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




        [HttpPost]
        public IActionResult UpdateGroup(int id, string ProjectName, string groupCode)
        {
            try
            {
                // Call the method to update the group code in the database
                var result = UpdateGroupInDatabase(id, ProjectName, groupCode);

                if (result)
                {
                    return Json(new { success = true, message = "Group Code updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update Group Code. Please try again." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating Group Code", error = ex.Message });
            }
        }

        private bool UpdateGroupInDatabase(int id, string ProjectName, string groupCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("dpr_sp_UpdateGroupCode", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters to the stored procedure
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@ProjectID", Convert.ToInt32(ProjectName));
                        command.Parameters.AddWithValue("@GroupCode", groupCode);

                        int rowsAffected = command.ExecuteNonQuery();

                        // Check if rows were affected (updated) in the database
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception here or handle it accordingly
                throw ex;
            }
        }









    }


}