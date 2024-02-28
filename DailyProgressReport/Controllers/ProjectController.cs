using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace DailyProgressReport.Controllers
{
    public class ProjectController : Controller
    {


        private readonly IConfiguration _configuration; // Inject your configuration if needed

        public ProjectController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            try
            {
                // Assuming GetProjects() returns a List<ProjectViewModel>
                List<ProjectViewModel> projects = GetProjects();
                List<ProjectType> Type = GetProjectTypes();
                ViewBag.ProjectTypes = GetProjectTypes();

                return View(projects);
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
        public ActionResult SaveProject(SaveProjectModel model, bool? IsLocationRequired,string LocationRequiredString)
        {
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPSaveProject", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.Add(new SqlParameter("@ProjectID", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    if (model.ProjectID == null)
                    {
                        command.Parameters.AddWithValue("@ProjectIDValue", DBNull.Value);

                    }
                    else
                    {
                        command.Parameters.AddWithValue("@ProjectIDValue", model.ProjectID); // Assuming model.ProjectID is an integer

                    }
                    command.Parameters.AddWithValue("@ProjectName", model.ProjectName);
                    command.Parameters.AddWithValue("@ProjectShortName", model.ProjectShortName);
                    command.Parameters.AddWithValue("@ProjectCode", model.ProjectCode);
                    command.Parameters.AddWithValue("@ProjectStartDate", cmn.ConvertDateFormatddmmyytommddyyDuringSave(model.ProjectStartDate));
                    command.Parameters.AddWithValue("@ProjectEndDate", cmn.ConvertDateFormatddmmyytommddyyDuringSave(model.ProjectEndDate));
                    command.Parameters.AddWithValue("@ProjectRevisedDate", cmn.ConvertDateFormatddmmyytommddyyDuringSave(model.ProjectRevisedDate));
                    //command.Parameters.AddWithValue("@IsLocationRequired", model.IsLocationRequired);
                    string locationRequiredString = model.IsLocationRequired ?? "false"; 
                    bool isLocationRequired = bool.TryParse(locationRequiredString, out bool result) ? result : true;
                    command.Parameters.AddWithValue("@IsLocationRequired", isLocationRequired);



                    command.ExecuteNonQuery();

                    // Retrieve the updated or newly inserted ProjectID
                    int updatedProjectID = Convert.ToInt32(command.Parameters["@ProjectID"].Value);
                    model.ProjectID = updatedProjectID;

                    connection.Close();
                }
            }

            return RedirectToAction("Index");
        }

        //public IActionResult Editproject(int? editProjectId)
        //{
        //    List<ProjectViewModel> projects = GetProjects();
        //    ViewBag.EditProjectId = editProjectId; // Pass the EditProjectId to the view
        //    return View(projects);
        //}

        public IActionResult EditProject(int projectId)
        {
            try
            {
                ProjectViewModel project = GetProjectById(projectId);
                return View("Index", new List<ProjectViewModel> { project });
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
        public ActionResult DeleteProject(int projectId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("dpr_SPDeleteProject", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.AddWithValue("@ProjectID", projectId);

                        // Execute the stored procedure
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting project", error = ex.Message });
            }
        }


        private ProjectViewModel GetProjectById(int projectId)
        {
            CommonFunction cmn = new CommonFunction();

            ProjectViewModel project = new ProjectViewModel();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tblProjects WHERE ProjectID = @ProjectID order by ProjectID desc", connection))
                {
                    command.Parameters.AddWithValue("@ProjectID", projectId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            project.ProjectID = Convert.ToInt32(reader["ProjectID"]);
                            project.ProjectName = reader["ProjectName"].ToString();
                            project.ProjectShortName = reader["ProjectShortName"].ToString();
                            project.ProjectCode = reader["ProjectCode"].ToString();
                            project.ProjectStartDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectStartDate"].ToString());
                            project.ProjectEndDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectEndDate"].ToString());
                            project.ProjectRevisedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectRevisedDate"].ToString());
                        }
                    }
                }
            }

            return project;
        }


        private List<ProjectViewModel> GetProjects()
        {
            CommonFunction cmn = new CommonFunction();
            List<ProjectViewModel> projects = new List<ProjectViewModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tblProjects", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProjectViewModel project = new ProjectViewModel
                            {
                                ProjectID = Convert.ToInt32(reader["ProjectID"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                ProjectShortName = reader["ProjectShortName"].ToString(),
                                ProjectCode = reader["ProjectCode"].ToString()
                                //ProjectStartDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectStartDate"].ToString()),
                                //ProjectEndDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectEndDate"].ToString()),
                                //ProjectRevisedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ProjectRevisedDate"].ToString())
                            };

                            projects.Add(project);
                        }
                    }
                }
            }

            return projects;
        }



        [HttpPost]
        public IActionResult ToggleProjectStatus(int ProjectID)
        {
            try
            {
                // Update the isActive flag in the database
                UpdateProjectStatus(ProjectID);

                // Return a JSON response (you can customize this as needed)
                return Json(new { success = true });
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

        private void UpdateProjectStatus(int ProjectID)
        {

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPUpdateProjectStatus", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProjectID", ProjectID);

                    command.ExecuteNonQuery();
                }
            }
        }
        [HttpPost]
        public ActionResult IsProjectCodeUnique(string projectCode)
        {
            bool isUnique = CheckProjectCodeUniquenessInDatabase(projectCode);

            return Json(isUnique);
        }
        private bool CheckProjectCodeUniquenessInDatabase(string projectCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))

                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM dpr_tblProjects WHERE ProjectCode = @ProjectCode";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectCode", projectCode);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        // If count is 0, the project code is unique; otherwise, it already exists
                        return count == 0;
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return false;
        }

        [HttpPost]
        public ActionResult UpdateCompletionStatus(int ProjectID, bool IsCompleted)
        {
            try
            {
                // Update the completion status in the database
                UpdateProjectCompletionStatus(ProjectID, IsCompleted);

                // Return a JSON response
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return Json(new { success = false, error = ex.Message });
            }
        }
        private void UpdateProjectCompletionStatus(int ProjectID, bool IsCompleted)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("UPDATE dpr_tblProjects SET IsCompleted = @IsCompleted WHERE ProjectID = @ProjectID", connection))
                    {
                        command.Parameters.AddWithValue("@IsCompleted", IsCompleted);
                        command.Parameters.AddWithValue("@ProjectID", ProjectID);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new Exception($"Failed to update completion status for project ID {ProjectID}.", ex);
            }
        }
   
        [HttpPost]
        public ActionResult AddLocation(int ProjectID, bool IsLocationRequired)
        {
            try
            {
                // Update the completion status in the database
                UpdateLocationStatus(ProjectID, IsLocationRequired);

                // Return a JSON response
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        private void UpdateLocationStatus(int ProjectID, bool IsLocationRequired)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("UPDATE dpr_tblProjects SET IsLocationRequired = @IsLocationRequired WHERE ProjectID = @ProjectID", connection))
                    {
                        command.Parameters.AddWithValue("@IsLocationRequired", IsLocationRequired);
                        command.Parameters.AddWithValue("@ProjectID", ProjectID);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new Exception($"Failed to update Location status for project ID {ProjectID}.", ex);
            }
        }
        private List<ProjectType> GetProjectTypes()
        {
            CommonFunction cmn = new CommonFunction();
            List<ProjectType> Types = new List<ProjectType>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_ProjectType", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProjectType Type = new ProjectType
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Value = reader["Value"].ToString(),


                            };

                            Types.Add(Type);
                        }
                    }
                }
            }

            return Types;
        }
        [HttpPost]
        public ActionResult validation(ProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index", "Project");


            }

            // If the model is not valid, return the view with validation errors
            return View(model);
        }
    }
}