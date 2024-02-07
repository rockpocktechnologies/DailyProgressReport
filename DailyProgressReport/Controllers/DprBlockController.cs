using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DailyProgressReport.Models;
using DailyProgressReport.Classes;
using System.Reflection;

namespace DailyProgressReport.Controllers
{
    public class DprBlockController : Controller
    {
        private readonly IConfiguration _configuration;

        public DprBlockController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                ShowBlocks model = new ShowBlocks();
                List<ProjectViewModel> projects = GetAssignedProjects();
                List<DprBlock> blocks = GetBlockListFromDatabase();
                model.projects = projects;
                model.blocks = blocks;
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

        [HttpPost]
        public IActionResult AddBlock(string blockName, int projectId)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");
                if (loggedInUserId != null && loggedInUserId != "")
                {
                    AddBlockToDatabase(blockName, projectId, loggedInUserId); // Replace "User123" with the actual user or logged-in user
                    return Json(new { success = true, message = "Block added successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = $"Error adding block, please login and try again." });

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

        public IActionResult BlockList()
        {
            List<DprBlock> blocks = GetBlockListFromDatabase();
            ViewBag.Blocks = blocks;
            return View();
        }

        [HttpGet]
        public IActionResult GetBlockById(int id)
        {
            try
            {
                DprBlock block = GetBlockByIdFromDatabase(id);
                return Json(new { success = true, block });
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
        public IActionResult UpdateBlock(int id, string blockName, int projectId)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null)
                {
                    UpdateBlockInDatabase(id, blockName, projectId, loggedInUserId); // Replace "User123" with the actual user or logged-in user
                    return Json(new { success = true, message = "Block updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = $"Error updating block, please login and try again" });

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
        public IActionResult DeleteBlock(int id)
        {
            try
            {
                DeleteBlockFromDatabase(id);
                return Json(new { success = true, message = "Block deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting block: {ex.Message}" });
            }
        }

        private void AddBlockToDatabase(string blockName, int projectId, string createdBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_AddBlock", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BlockName", blockName);
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        private List<DprBlock> GetBlockListFromDatabase()
        {

           
                List<DprBlock> blocks = new List<DprBlock>();
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
                                DprBlock block = new DprBlock
                                {
                                    Id = (int)reader["Id"],
                                    BlockName = reader["BlockName"].ToString(),
                                    ProjectId = (int)reader["ProjectId"],
                                    ProjectName = reader["ProjectName"].ToString(),
                                    // Add other properties as needed
                                    CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                    CreatedBy = reader["CreatedBy"].ToString(),
                                    UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                    UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                                };

                                blocks.Add(block);
                            }
                        }
                    }
                }
            }

            return blocks;
        }


        private DprBlock GetBlockByIdFromDatabase(int id)
        {
            DprBlock block = new DprBlock();
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_block WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            block.Id = (int)reader["Id"];
                            block.BlockName = reader["BlockName"].ToString();
                            block.ProjectId = (int)reader["ProjectId"];
                            // Add other properties as needed
                            block.CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString());
                            block.CreatedBy = reader["CreatedBy"].ToString();
                            block.UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "";
                            block.UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null;
                        }
                    }
                }
            }

            return block;
        }

        private void UpdateBlockInDatabase(int id, string blockName, int projectId, string modifiedBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateBlock", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@BlockName", blockName);
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    command.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

                    command.ExecuteNonQuery();
                }
            }
        }


        private void DeleteBlockFromDatabase(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DELETE FROM dpr_block WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
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
    }
}
