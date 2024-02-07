// Add necessary using statements

using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using DailyProgressReport.Models;
using DailyProgressReport.Classes;

namespace DailyProgressReport.Controllers
{
    public class DprLocationController : Controller
    {
        private readonly IConfiguration _configuration;

        // Constructor to inject IConfiguration
        public DprLocationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                ShowLocations model = new ShowLocations();
                List<DprBlock> blocks = GetBlockListFromDatabase(loggedInUserId);
                List<DprComponent> components = GetComponentListFromDatabase(loggedInUserId);
                List<DprLocation> location = GetLocationsFromDatabase(loggedInUserId);
                model.components = components;
                model.blocks = blocks;
                model.locations = location;
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

        private List<DprBlock> GetBlockListFromDatabase(string loggedInUserId)
        {


            List<DprBlock> blocks = new List<DprBlock>();

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


        private List<DprComponent> GetComponentListFromDatabase(string loggedInUserId)
        {
            CommonFunction cmn = new CommonFunction();
            List<DprComponent> components = new List<DprComponent>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetComponentList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CreatedBy", loggedInUserId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DprComponent component = new DprComponent
                            {
                                Id = (int)reader["Id"],
                                ComponentName = reader["ComponentName"].ToString(),
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                // Add other properties as needed
                            };

                            components.Add(component);
                        }
                    }
                }
            }

            return components;
        }


        // Add your existing code for other actions

        [HttpPost]
        public IActionResult AddLocation(int componentId, int blockId, string locationName)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null)
                {
                    AddLocationToDatabase(componentId, blockId, locationName, loggedInUserId);
                    return Json(new { success = true, message = "Location added successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error adding location, please login and try again" });
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

        private void AddLocationToDatabase(int componentId, int blockId, string locationName, string createdBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_AddLocation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ComponentId", componentId);
                    command.Parameters.AddWithValue("@BlockId", blockId);
                    command.Parameters.AddWithValue("@LocationName", locationName);
                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    command.ExecuteNonQuery();
                }
            }
        }


        [HttpPost]
        public IActionResult UpdateLocation(int id, int componentId, int blockId, string locationName)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                if (loggedInUserId != null)
                {
                    UpdateLocationInDatabase(id, componentId, blockId, locationName, loggedInUserId);
                    return Json(new { success = true, message = "Location updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error updating location, please login and try again" });
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
        public IActionResult DeleteLocation(int id)
        {
            try
            {
                DeleteLocationFromDatabase(id);
                return Json(new { success = true, message = "Location deleted successfully!" });
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

        //public IActionResult ShowLocations()
        //{
        //    try
        //    {
        //        List<DprLocation> locations = GetLocationsFromDatabase();
        //        return View(new ShowLocations { locations = locations });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"Error fetching locations: {ex.Message}" });
        //    }
        //}

        private List<DprLocation> GetLocationsFromDatabase(string loggedInUserId)
        {
            List<DprLocation> locations = new List<DprLocation>();
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetLocations", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", loggedInUserId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DprLocation location = new DprLocation
                            {
                                Id = (int)reader["Id"],
                                ComponentId = (int)reader["ComponentId"],
                                BlockId = (int)reader["BlockId"],
                                BlockName = reader["BlockName"].ToString(),
                                ComponentName = reader["ComponentName"].ToString(),
                                LocationName = reader["LocationName"].ToString(),
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                                // Add other properties as needed
                            };

                            locations.Add(location);
                        }
                    }
                }
            }

            return locations;
        }

        [HttpGet]
        public IActionResult GetLocationById(int id)
        {
            try
            {
                // Implement logic to fetch location details by ID from the database
                DprLocation location = GetLocationByIdFromDatabase(id);

                if (location != null)
                {
                    return Json(new { success = true, location = location });
                }
                else
                {
                    return Json(new { success = false, message = "Location not found" });
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

        private DprLocation GetLocationByIdFromDatabase(int id)
        {
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetLocationById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DprLocation location = new DprLocation
                            {
                                Id = (int)reader["Id"],
                                ComponentId = (int)reader["ComponentId"],
                                BlockId = (int)reader["BlockId"],
                                LocationName = reader["LocationName"].ToString(),
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                                // Add other properties as needed
                            };

                            return location;
                        }
                    }
                }

                return null; // Return null if the location with the specified ID is not found
            }
        }

        private void UpdateLocationInDatabase(int id, int componentId, int blockId, string locationName, string modifiedBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateLocation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ComponentId", componentId);
                    command.Parameters.AddWithValue("@BlockId", blockId);
                    command.Parameters.AddWithValue("@LocationName", locationName);
                    command.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteLocationFromDatabase(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_DeleteLocation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Add your other actions and methods
    }
}
