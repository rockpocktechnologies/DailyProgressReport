using Microsoft.AspNetCore.Mvc;
using DailyProgressReport.Models;
using System.Data.SqlClient;
using System.Data;
using DailyProgressReport.Classes;

    namespace DailyProgressReport {
    public class DprVillageController : Controller
    {
        private readonly IConfiguration _configuration;

        // Constructor to inject IConfiguration
        public DprVillageController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                ShowVillage model = new ShowVillage();
                List<DprLocation> locations = GetLocationsFromDatabase(loggedInUserId);
                List<DprVillageModel> villages = GetVillageListFromDatabase(loggedInUserId);
                model.locations = locations;
                model.villages = villages;
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


        // Action to add a new village
        [HttpPost]
        public IActionResult AddVillage(int locationId, string villageName)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                AddVillageToDatabase(locationId, villageName, loggedInUserId);
                return Json(new { success = true, message = "Village added successfully!" });
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

        // Action to get the list of villages
        [HttpGet]
        public IActionResult GetVillageList()
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");
                List<DprVillageModel> villages = GetVillageListFromDatabase(loggedInUserId);
                return Json(new { success = true, villages = villages });
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

        // Action to get village details by ID
        [HttpGet]
        public IActionResult GetVillageById(int id)
        {
            try
            {
                DprVillageModel village = GetVillageByIdFromDatabase(id);
                if (village != null)
                {
                    return Json(new { success = true, village = village });
                }
                else
                {
                    return Json(new { success = false, message = "Village not found" });
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

        // Action to update village details
        [HttpPost]
        public IActionResult UpdateVillage(int id, int locationId,string villageName)
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                UpdateVillageInDatabase(id, locationId, villageName, loggedInUserId);
                return Json(new { success = true, message = "Village updated successfully!" });
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

        private void UpdateVillageInDatabase(int id, int locationId, string villageName, string modifiedBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateVillage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@LocationId", locationId);
                    command.Parameters.AddWithValue("@VillageName", villageName);
                    command.Parameters.AddWithValue("@UpdatedBy", modifiedBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Action to delete a village
        [HttpPost]
        public IActionResult DeleteVillage(int id)
        {
            try
            {
                DeleteVillageFromDatabase(id);
                return Json(new { success = true, message = "Village deleted successfully!" });
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

        private void AddVillageToDatabase(int locationId, string villageName, string createdBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_AddVillage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LocationId", locationId);
                    command.Parameters.AddWithValue("@VillageName", villageName);
                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    command.ExecuteNonQuery();
                }
            }
        }

        private List<DprVillageModel> GetVillageListFromDatabase(string loggedInUserId)
        {
            List<DprVillageModel> villages = new List<DprVillageModel>();
            CommonFunction cmn = new CommonFunction();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetVillageList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", loggedInUserId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DprVillageModel village = new DprVillageModel
                            {
                                Id = (int)reader["Id"],
                                VillageName = reader["VillageName"].ToString(),
                                LocationName = reader["LocationName"].ToString(),
                                BlockName = reader["BlockName"].ToString(),
                                ComponentId = (int)reader["ComponentId"],
                                BlockId = (int)reader["BlockId"],
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                            };

                            villages.Add(village);
                        }
                    }
                }
            }

            return villages;
        }

        private DprVillageModel GetVillageByIdFromDatabase(int id)
        {

            CommonFunction cmn = new CommonFunction();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetVillageById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DprVillageModel village = new DprVillageModel
                            {
                                Id = (int)reader["Id"],
                                LocationId = (int)reader["LocationId"],
                                VillageName = reader["VillageName"].ToString(),
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) :"",
                                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                                // Add other properties as needed
                            };

                            return village;
                        }
                    }
                }
            }

            return null;
        }

        private void DeleteVillageFromDatabase(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_DeleteVillage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Other helper methods...
    }

}