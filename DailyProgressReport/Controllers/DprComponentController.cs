using DailyProgressReport.Classes;
using DailyProgressReport.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;


 
namespace DailyProgressReport.Controllers

{

    public class DprComponentController : Controller

    {

        private readonly IConfiguration _configuration;

        public DprComponentController(IConfiguration configuration)

        {

            _configuration = configuration;

        }

        public IActionResult Index()

        {

            try

            {

                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                ShowComponent component = new ShowComponent();

                List<ProjectViewModel> project = GetAssignedProjects();

                ViewBag.ProjectsData = new SelectList(project, "ProjectID", "ProjectName");

                List<DprComponent> ListCompo = GetComponentListFromDatabase(loggedInUserId);

                List<DprComponent> compo = new List<DprComponent>();

                component.projects = project;

                component.compo = compo;

                component.compo = ListCompo;


                if (loggedInUserId != null && loggedInUserId != "")

                {

                    compo = GetComponentListFromDatabase(loggedInUserId);

                }

                return View(component);

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

        //                        ProjectShortName = reader["ProjectShortName"].ToString(),

        //                        ProjectCode = reader["ProjectCode"].ToString(),

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

        public IActionResult AddComponent(string ProjectName, string componentName)

        {

            try

            {

                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null && loggedInUserId != "")

                {

                    AddComponentToDatabase(ProjectName, componentName, loggedInUserId); // Replace "User123" with the actual user or logged-in user

                    return Json(new { success = true, message = "Component added successfully!" });

                }

                else

                {

                    return Json(new { success = false, message = $"Error adding component, please login and retry." });

                }

                // Handle the addition of the component to the database

                // Return success message as JSON

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

        public IActionResult ComponentList()

        {

            try

            {

                // Fetch component list from the database

                List<DprComponent> components = new List<DprComponent>();

                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null && loggedInUserId != "")

                {

                    components = GetComponentListFromDatabase(loggedInUserId);

                }

                // Pass the components to the view

                ViewBag.Components = components;

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

        private void AddComponentToDatabase(string ProjectName, string componentName, string createdBy)

        {

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))

            {

                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_AddComponent", connection))

                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ProjectID ", Convert.ToInt32(ProjectName));

                    command.Parameters.AddWithValue("@ComponentName", componentName);

                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    command.ExecuteNonQuery();

                }

            }

        }
        private List<DprComponent> GetComponentListFromDatabase(string loggedInUserId)

        {

            CommonFunction cmn = new CommonFunction();

            List<DprComponent> components = new List<DprComponent>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))

            {

                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetComponentList", connection))

                {

                    //   dpr_sp_TempGetComponentList SP

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@CreatedBy", loggedInUserId);

                    using (SqlDataReader reader = command.ExecuteReader())

                    {

                        while (reader.Read())

                        {

                            DprComponent component = new DprComponent

                            {

                                Id = Convert.ToInt32(reader["Id"]),

                                // Check for DBNull before converting to int

                                // ProjectID = reader["ProjectID"] != DBNull.Value ? Convert.ToInt32(reader["ProjectID"]) : (int?)null,

                                // ProjectName = reader["ProjectName"].ToString(),

                                ComponentName = reader["ComponentName"].ToString(),

                                // CreatedDate = reader["CreatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()) : "",

                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),

                                CreatedBy = reader["CreatedBy"].ToString(),

                                ModifiedBy = reader["ModifiedBy"] != DBNull.Value ? reader["ModifiedBy"].ToString() : null,

                                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["ModifiedDate"].ToString()) : "",

                                // Add other properties as needed

                            };

                            components.Add(component);

                        }

                    }

                }

            }

            return components;

        }

        // ...

        [HttpGet]

        public IActionResult GetComponentById(int id)

        {

            try

            {

                DprComponent component = GetComponentByIdFromDatabase(id);

                return Json(new { success = true, component });

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

        public IActionResult UpdateComponent(int id, string projectName, string componentName)

        {

            try

            {

                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null && loggedInUserId != "")

                {

                    UpdateComponentInDatabase(id, projectName, componentName, loggedInUserId); // Replace "User123" with the actual user or logged-in user

                    return Json(new { success = true, message = "Component updated successfully!" });

                }

                else

                {

                    return Json(new { success = false, message = $"Error updating component, please login and try again" });

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

        public IActionResult DeleteComponent(int id)

        {

            try

            {

                DeleteComponentFromDatabase(id);

                return Json(new { success = true, message = "Component deleted successfully!" });

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

        private DprComponent GetComponentByIdFromDatabase(int id)

        {

            DprComponent component = new DprComponent();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))

            {

                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetComponentById", connection))

                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())

                    {

                        if (reader.Read())

                        {

                            component.Id = Convert.ToInt32(reader["Id"]);

                            //component.ProjectID = (int)reader["ProjectID"];

                            component.ComponentName = reader["ComponentName"].ToString();

                            // Add other properties as needed

                        }

                    }

                }

            }

            return component;

        }

        private void UpdateComponentInDatabase(int id, string projectName, string componentName, string modifiedBy)

        {

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))

            {

                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateComponent", connection))

                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", id);

                    command.Parameters.AddWithValue("@ProjectID", Convert.ToInt32(projectName));

                    command.Parameters.AddWithValue("@ComponentName", componentName);

                    command.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

                    command.ExecuteNonQuery();

                }

            }

        }

        private void DeleteComponentFromDatabase(int id)

        {

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))

            {

                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_DeleteComponent", connection))

                {

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();

                }

            }

        }






    }

}