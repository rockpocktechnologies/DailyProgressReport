using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace DailyProgressReport.Controllers
{
    public class AssignProjectController : Controller
    {
        
            private readonly IConfiguration _configuration;

            public AssignProjectController(IConfiguration configuration)
            {
                _configuration = configuration;
            }

        public IActionResult Index()
        {
            try
            {
                List<ProjectViewModel> projects = GetProjectsFromDatabase();
                ViewBag.ProjectsData = new SelectList(projects, "ProjectID", "ProjectName");
                return View();
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

          //public IActionResult AssignProjects()
          //  {
          //      List<ProjectViewModel> projects = GetProjectsFromDatabase();
          //      ViewBag.Projects = new SelectList(projects, "ProjectID", "ProjectName");
          //      return View();
          //  }

            private List<ProjectViewModel> GetProjectsFromDatabase()
            {
                List<ProjectViewModel> projects = new List<ProjectViewModel>();

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tblProjects ORDER BY ProjectID DESC", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProjectViewModel project = new ProjectViewModel
                                {
                                    ProjectID = Convert.ToInt32(reader["ProjectID"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    // Add other properties as needed
                                };

                                projects.Add(project);
                            }
                        }
                    }
                }

                return projects;
            }

        [HttpPost]
        public IActionResult AssignProject(AssignProjectViewModel viewModel)
        {
            try
            {
                if (viewModel.ProjectId > 0 && viewModel.adminIds != null && viewModel.adminIds.Any())
                {
                    // Call the method to save assignment to the database
                    SaveAssignment(viewModel.ProjectId, viewModel.adminIds);
                    return Json(new { success = true, message = "Project assigned successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid parameters for project assignment." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return Json(new { success = false, message = "Error assigning project: " + ex.Message });
            }
        }

        private void SaveAssignment(int projectId, List<string> adminIds)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPAssignProjectToAdmin", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@ProjectId", projectId);

                    // Create a DataTable for the list of admin IDs
                    DataTable adminIdTable = new DataTable();
                    adminIdTable.Columns.Add("AdminId", typeof(string));

                    foreach (string adminId in adminIds)
                    {
                        adminIdTable.Rows.Add(adminId);
                    }

                    // Add table-valued parameter for admin IDs
                    SqlParameter adminIdsParam = command.Parameters.AddWithValue("@AdminIds", adminIdTable);
                    adminIdsParam.SqlDbType = SqlDbType.Structured;
                    adminIdsParam.TypeName = "AdminIdTableType"; // Use the correct type name

                    command.ExecuteNonQuery();
                }
            }
        }

        public ActionResult AssignmentHistory()
        {
            List<AssignmentHistoryViewModel> assignmentHistory = GetAssignmentHistoryFromDatabase();
            return Json(assignmentHistory);
        }

        private List<AssignmentHistoryViewModel> GetAssignmentHistoryFromDatabase()
        {
            List<AssignmentHistoryViewModel> assignmentHistory = new List<AssignmentHistoryViewModel>();
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPGetAssignmentHistory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AssignmentHistoryViewModel historyItem = new AssignmentHistoryViewModel
                            {
                                AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                AdminName = reader["AdminName"].ToString(),
                                AssignmentDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay((reader["AssignmentDate"]).ToString())
                            };

                            assignmentHistory.Add(historyItem);
                        }
                    }
                }
            }

            return assignmentHistory;
        }

        [HttpGet]
        public IActionResult GetAdminNames(string term)
        {
            try
            {           
                List<AdminModel> adminNames = GetAllADUsers(term);
                return Json(adminNames);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return BadRequest("Error fetching admin names.");
            }
        }

        private List<AdminModel> GetAllADUsers(string searchPattern)
        {
            List<AdminModel> adminNames = new List<AdminModel>();

            using (var context = new PrincipalContext(ContextType.Domain, "AFCONSINFRA"))
            {
                UserPrincipal qbeUser = new UserPrincipal(context);
                qbeUser.UserPrincipalName = "*" + searchPattern.ToLower().Trim() + "*";

                using (var searcher = new PrincipalSearcher(qbeUser))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        if (result != null)
                        {
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                            AdminModel admin = new AdminModel
                            {
                                Name = de.Properties["givenName"].Value + " " + de.Properties["sn"].Value,
                                UserId = de.Properties["samAccountName"].Value.ToString(),
                                Email = de.Properties["userPrincipalName"].Value.ToString()
                            };

                            adminNames.Add(admin);
                        }
                        else
                        {
                            // Handle case where no records are found
                        }
                    }
                }
            }

            return adminNames;
        }
    }

}

