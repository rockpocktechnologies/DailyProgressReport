using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace DailyProgressReport.Controllers
{
    public class PlanningManagerController : Controller
    {

        private readonly IConfiguration _configuration;

        public PlanningManagerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                //string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                //PlanningManagerModel planning = new PlanningManagerModel();
                //List<ProjectViewModel> project = GetProjectsFromDatabase();
                //ViewBag.ProjectsData = new SelectList(project, "ProjectID", "ProjectName");
                List<PlanningManagerModel> plan = new List<PlanningManagerModel>();
                //planning.Projects = project.ToList();
                //sp.Planning = plan;
                //if (loggedInUserId != null && loggedInUserId != "")
                //{
                //    plan = GetProjectListFromDatabase(loggedInUserId);
                //}
                //return View(project);
                ShowPlanningModel sp = new ShowPlanningModel();

                List<ProjectViewModel> projects = GetAssignedProjects();
                ViewBag.ProjectsData = new SelectList(projects, "ProjectID", "ProjectName");
                sp.projects = projects;
                List<PlanningManagerModel> Planning = GetPlanningFromDatabase();

                sp.Planning = Planning;
                // sp.Planning = plan;
                return View(sp);
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
                            };

                            projects.Add(project);
                        }
                    }
                }
            }

            return projects;
        }

        [HttpPost]
        public IActionResult AssignProject(PlanningManagerModel viewModel)
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
        public IActionResult GetSiteEngineerName(string term)
        {
            try
            {
                List<SiteEnggModel> SiteEngineerName = GetAllADUsers(term);
                return Json(SiteEngineerName);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return BadRequest("Error fetching SiteEngineer names.");
            }
        }



        private List<SiteEnggModel> GetAllADUsers(string searchPattern)
        {
            List<SiteEnggModel> SiteEngineerName = new List<SiteEnggModel>();

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

                            SiteEngineerName.Add(SiteEngineer);
                        }
                        else
                        {
                            // Handle case where no records are found
                        }
                    }
                }
            }

            return SiteEngineerName;
        }
        private List<PlanningManagerModel> GetPlanningProject()
        {
            CommonFunction cmn = new CommonFunction();
            List<PlanningManagerModel> projects = new List<PlanningManagerModel>();

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
                                PlanningManagerModel project = new PlanningManagerModel
                                {
                                    ProjectId = Convert.ToInt32(reader["ProjectId"]),
                                    SiteEngineerId = reader["SiteEngineerId"].ToString(),
                                    Projects = reader["Projects"].ToString(),
                                    SiteEngineerName = reader["SiteEngineerName"].ToString(),

                                };

                                projects.Add(project);
                            }
                        }
                    }
                }
            }

            return projects;
        }
        private List<PlanningManagerModel> GetPlanningFromDatabase()
        {
            CommonFunction cmn = new CommonFunction();

            List<PlanningManagerModel> projects = new List<PlanningManagerModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT ProjectId,SiteEngineerId,Projects,SiteEngineerName,SiteEngineerIds,AssignmentDate FROM dpr_tblPlanningManager ORDER BY ProjectID DESC", connection))
                //using (SqlCommand command = new SqlCommand("dpr_SPAssignProjectToSiteEngineer", connection))

                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PlanningManagerModel project = new PlanningManagerModel
                            {
                                ProjectId = Convert.ToInt32(reader["ProjectId"]),
                                Projects = reader["Projects"].ToString(),
                                SiteEngineerId = reader["SiteEngineerId"].ToString(),
                                SiteEngineerName = reader["SiteEngineerName"].ToString(),
                                AssignmentDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["AssignmentDate"].ToString()),

                                // Add other properties as needed
                            };

                            projects.Add(project);
                        }
                    }
                }
            }

            return projects;
        }
        //        [HttpPost]
        //        public ActionResult SaveProject(SaveProjectModel model)
        //        {
        //            CommonFunction cmn = new CommonFunction();

        //            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
        //            {
        //                connection.Open();

        //                using (SqlCommand command = new SqlCommand("dpr_SPSaveProject", connection))
        //                {
        //                    command.CommandType = CommandType.StoredProcedure;

        //                    // Add parameters
        //                    command.Parameters.Add(new SqlParameter("@ProjectID", SqlDbType.Int) { Direction = ParameterDirection.Output });
        //                    if (model.ProjectID == null)
        //                    {
        //                        command.Parameters.AddWithValue("@ProjectIDValue", DBNull.Value);

        //                    }
        //                    else
        //                    {
        //                        command.Parameters.AddWithValue("@ProjectIDValue", model.ProjectID); // Assuming model.ProjectID is an integer

        //                    }
        //                    command.Parameters.AddWithValue("@ProjectName", model.ProjectName);
        //                    command.Parameters.AddWithValue("@ProjectShortName", model.ProjectShortName);
        //                    command.Parameters.AddWithValue("@ProjectCode", model.ProjectCode);
        //                    command.Parameters.AddWithValue("@ProjectStartDate", cmn.ConvertDateFormatddmmyytommddyyDuringSave(model.ProjectStartDate));
        //                    command.Parameters.AddWithValue("@ProjectEndDate", cmn.ConvertDateFormatddmmyytommddyyDuringSave(model.ProjectEndDate));
        //                    command.Parameters.AddWithValue("@ProjectRevisedDate", cmn.ConvertDateFormatddmmyytommddyyDuringSave(model.ProjectRevisedDate));


        //                    command.ExecuteNonQuery();

        //                    // Retrieve the updated or newly inserted ProjectID
        //                    int updatedProjectID = Convert.ToInt32(command.Parameters["@ProjectID"].Value);
        //                    model.ProjectID = updatedProjectID;

        //                    connection.Close();
        //                }
        //            }

        //            return RedirectToAction("Index");
        //        }

        //        //public IActionResult Editproject(int? editProjectId)
        //        //{
        //        //    List<ProjectViewModel> projects = GetProjects();
        //        //    ViewBag.EditProjectId = editProjectId; // Pass the EditProjectId to the view
        //        //    return View(projects);
        //        //}
        [HttpPost]
        public IActionResult editPlanning(int projectId)
        {
            try
            {
                PlanningManagerModel pl = GetProjectById(projectId);
                return View("Index", new List<PlanningManagerModel> { pl });
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
        public ActionResult DeletePlanning(int ProjectId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();
                    string sqlQuery = "DELETE FROM dpr_tblPlanningManager WHERE ProjectId = @ProjectId";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectId", ProjectId);
                        command.ExecuteNonQuery();
                    }
                    //using (SqlCommand command = new SqlCommand("tblPlanningManager", connection))
                    //{
                    //    command.CommandType = CommandType.StoredProcedure;

                    //    // Add parameters
                    //    command.Parameters.AddWithValue("@ProjectId", ProjectId);

                    //    // Execute the stored procedure
                    //    command.ExecuteNonQuery();
                    //}
                }

                return Json(new { success = true, message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting project", error = ex.Message });
            }
        }
        private PlanningManagerModel GetProjectById(int ProjectId)
        {
            CommonFunction cmn = new CommonFunction();

            PlanningManagerModel project = new PlanningManagerModel();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT  ProjectId,SiteEngineerId,Projects,SiteEngineerName,AssignmentDate FROM dpr_tblPlanningManager WHERE ProjectId = @ProjectId order by ProjectId desc", connection))
                //using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tblPlanningManager WHERE ProjectId = @ProjectId order by ProjectId desc", connection))

                {
                    command.Parameters.AddWithValue("@ProjectId", ProjectId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            project.ProjectId = Convert.ToInt32(reader["ProjectId"]);
                            project.SiteEngineerId = reader["SiteEngineerId"].ToString();
                            project.Projects = reader["Projects"].ToString();
                            project.SiteEngineerName = reader["SiteEngineerName"].ToString();
                            project.AssignmentDate = reader["AssignmentDate"].ToString();

                        }
                    }
                }
            }

            return project;
        }

        [HttpPost]
        public IActionResult UpdatePlanning(int ProjectId)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");
                if (loggedInUserId != null && loggedInUserId != "")
                {
                    UpdatePlanningInDatabase(ProjectId); // Replace "User123" with the actual user or logged-in user
                    return Json(new { success = true, message = "Data updated successfully!" });

                }
                else
                {
                    return Json(new { success = false, message = $"Error updating Data, please login and try again" });

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
        private void UpdatePlanningInDatabase(int ProjectId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_tblPlanningManager", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProjectId", ProjectId);
                    //command.Parameters.AddWithValue("@ProjectName", ProjectName);
                    //command.Parameters.AddWithValue("@SiteEngineerName", SiteEngineerName);
                    //.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

                    command.ExecuteNonQuery();
                }
            }
        }
        //        private List<ProjectViewModel> GetProjects()
        //        {
        //            CommonFunction cmn = new CommonFunction();
        //            List<ProjectViewModel> projects = new List<ProjectViewModel>();

        //            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
        //            {
        //                connection.Open();

        //                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tblProjects", connection))
        //                {
        //                    using (SqlDataReader reader = command.ExecuteReader())
        //                    {
        //                        while (reader.Read())
        //                        {
        //                            ProjectViewModel project = new ProjectViewModel
        //                            {
        //                                ProjectID = Convert.ToInt32(reader["ProjectID"]),
        //                                ProjectName = reader["ProjectName"].ToString(),
        //                                ProjectShortName = reader["ProjectShortName"].ToString(),
        //                                ProjectCode = reader["ProjectCode"].ToString()
        //                                //ProjectStartDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectStartDate"].ToString()),
        //                                //ProjectEndDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectEndDate"].ToString()),
        //                                //ProjectRevisedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectRevisedDate"].ToString())
        //                            };

        //                            projects.Add(project);
        //                        }
        //                    }
        //                }
        //            }

        //            return projects;
        //        }
        [HttpGet]
        public IActionResult GetSightEngineerNames(string term)
        {
            try
            {
                List<AdminModel> adminNames = GetAllSightEngineers(term);
                return Json(adminNames);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return BadRequest("Error fetching admin names.");
            }
        }
        private List<AdminModel> GetAllSightEngineers(string searchPattern)
        {
            List<AdminModel> adminNames = new List<AdminModel>();

            using (var context = new PrincipalContext(ContextType.Domain, "AFCONSINFRA"))
            {
                UserPrincipal qbeUser = new UserPrincipal(context);
                qbeUser.UserPrincipalName = "*" + searchPattern.ToLower().Trim() + "*";
                qbeUser.Enabled = true; // You may want to filter only enabled users

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
