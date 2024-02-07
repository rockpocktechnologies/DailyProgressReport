using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DailyProgressReport.Models;
using DailyProgressReport.Classes;

namespace DailyProgressReport.Controllers
{
    [Route("[controller]/[action]")]
    public class DprBOQHeadController : Controller
    {
        private readonly IConfiguration _configuration;

        public DprBOQHeadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            try
            {
                ShowBOQHeads model = new ShowBOQHeads();
                List<ProjectViewModel> projects = GetAssignedProjects();
                List<DprBOQHead> boq = GetBOQHeadListFromDatabase();
                model.projects = projects;
                model.boq = boq;
                return View(model);
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
        public IActionResult AddBOQHead(string boqHeadName)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null)
                {
                    AddBOQHeadToDatabase(boqHeadName, loggedInUserId);
                    return Json(new { success = true, message = "BOQ Head added successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error adding BOQ Head, please login and try again" });
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

        [HttpGet]
        public IActionResult GetBOQHeadById(int id)
        {
            try
            {
                DprBOQHead boqHead = GetBOQHeadByIdFromDatabase(id);
                return Json(new { success = true, boqHead = boqHead });
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
        public IActionResult UpdateBOQHead(int id, string boqHeadName)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null)
                {
                    UpdateBOQHeadInDatabase(id, boqHeadName, loggedInUserId);
                    return Json(new { success = true, message = "BOQ Head updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error updating BOQ Head, please login and try again" });
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
        public IActionResult DeleteBOQHead(int id)
        {
            try
            {
                DeleteBOQHeadFromDatabase(id);
                return Json(new { success = true, message = "BOQ Head deleted successfully!" });
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


        private List<DprBOQHead> GetBOQHeadListFromDatabase()
        {
            List<DprBOQHead> boqHeads = new List<DprBOQHead>();
            CommonFunction cmn = new CommonFunction();
            string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetBOQHeadList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", loggedInUserId);


                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DprBOQHead boqHead = new DprBOQHead
                            {
                                Id = (int)reader["Id"],
                                BOQHeadName = reader["BOQHeadName"].ToString(),
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                            };

                            boqHeads.Add(boqHead);
                        }
                    }
                }
            }

            return boqHeads;
        }

        private DprBOQHead GetBOQHeadByIdFromDatabase(int id)
        {
            DprBOQHead boqHead = null;
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetBOQHeadById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            boqHead = new DprBOQHead
                            {
                                Id = (int)reader["Id"],
                                BOQHeadName = reader["BOQHeadName"].ToString(),
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                            };
                        }
                    }
                }
            }

            return boqHead;
        }

        private void AddBOQHeadToDatabase(string boqHeadName, string createdBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_AddBOQHead", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BOQHeadName", boqHeadName);
                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void UpdateBOQHeadInDatabase(int id, string boqHeadName, string updatedBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateBOQHead", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@BOQHeadName", boqHeadName);
                    command.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteBOQHeadFromDatabase(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_DeleteBOQHead", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
            }
        }


    }
}
