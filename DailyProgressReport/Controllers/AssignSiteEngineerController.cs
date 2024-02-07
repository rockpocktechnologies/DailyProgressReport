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
    public class AssignSiteEngineerController : Controller
    {

        private readonly IConfiguration _configuration;

        public AssignSiteEngineerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                List<ProjectViewModel> projects = GetAssignedProjects();
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

                    using (SqlCommand command = new SqlCommand("dpr_SPGetAssignedProjectsToSiteEngg", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@SiteEngineerId", loggedInUserId);

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
        public IActionResult AssignProject(AssignSiteEngineerModel viewModel)
        {
            try
            {
                if (viewModel.ProjectId > 0 && viewModel.SiteEngineerIds != null && viewModel.SiteEngineerIds.Any())
                {
                    // Call the method to save assignment to the database
                    SaveAssignment(viewModel.ProjectId, viewModel.SiteEngineerIds);
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

        private void SaveAssignment(int projectId, List<string> SiteEngineerIds)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPAssignProjectToSiteEngineer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@ProjectId", projectId);

                    // Create a DataTable for the list of SiteEngineer IDs
                    DataTable SiteEngineerIdTable = new DataTable();
                    SiteEngineerIdTable.Columns.Add("SiteEngineerId", typeof(string));

                    foreach (string SiteEngineerId in SiteEngineerIds)
                    {
                        SiteEngineerIdTable.Rows.Add(SiteEngineerId);
                    }

                    // Add table-valued parameter for SiteEngineer IDs
                    SqlParameter SiteEngineerIdsParam = command.Parameters.AddWithValue("@SiteEngineerIds", SiteEngineerIdTable);
                    SiteEngineerIdsParam.SqlDbType = SqlDbType.Structured;
                    SiteEngineerIdsParam.TypeName = "SiteEngineerIdTableType"; // Use the correct type name

                    command.ExecuteNonQuery();
                }
            }
        }

        public ActionResult AssignmentHistory()
        {
            List<SiteEnggAssignmentHistoryViewModel> assignmentHistory = GetAssignmentHistoryFromDatabase();
            return Json(assignmentHistory);
        }

        private List<SiteEnggAssignmentHistoryViewModel> GetAssignmentHistoryFromDatabase()
        {
            List<SiteEnggAssignmentHistoryViewModel> assignmentHistory = new List<SiteEnggAssignmentHistoryViewModel>();
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPGetAssignmentHistoryOfSiteEngg", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SiteEnggAssignmentHistoryViewModel historyItem = new SiteEnggAssignmentHistoryViewModel
                            {
                                AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                SiteEngineerName = reader["SiteEngineerName"].ToString(),
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
        public IActionResult GetSiteEngineerNames(string term)
        {
            try
            {
                List<SiteEnggModel> SiteEngineerNames = GetAllADUsers(term);
                return Json(SiteEngineerNames);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return BadRequest("Error fetching SiteEngineer names.");
            }
        }



        private List<SiteEnggModel> GetAllADUsers(string searchPattern)
        {
            List<SiteEnggModel> SiteEngineerNames = new List<SiteEnggModel>();

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
                            SiteEnggModel SiteEngineer = new SiteEnggModel
                            {
                                Name = de.Properties["givenName"].Value + " " + de.Properties["sn"].Value,
                                UserId = de.Properties["samAccountName"].Value.ToString(),
                                Email = de.Properties["userPrincipalName"].Value.ToString()
                            };

                            SiteEngineerNames.Add(SiteEngineer);
                        }
                        else
                        {
                            // Handle case where no records are found
                        }
                    }
                }
            }

            return SiteEngineerNames;
        }
    }

}