using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DailyProgressReport.Models;
using DailyProgressReport.Classes;

namespace DailyProgressReport.Controllers
{
    public class AddMasterController : Controller
    {
        private readonly IConfiguration _configuration;

        public AddMasterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            try
            {
                List<Master> masterTypes = GetMasterTypesFromDatabase();

                ViewBag.MasterTypes = masterTypes;

                return View();
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string exceptionMessage = $"Exception in method '{methodName}'";

                var errorLogger = new CustomErrorLog(_configuration);
                errorLogger.LogError(ex, exceptionMessage);
                return Json(new { error = ex.Message });
            }
            
        }

        private List<Master> GetMasterTypesFromDatabase()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            List<Master> masterTypes = new List<Master>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPGetMasterTypes", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Master master = new Master
                            {
                                Id = (int)reader["Id"],
                                Name = reader["MasterType"].ToString(),
                            };

                            masterTypes.Add(master);
                        }
                    }
                }
            }

            return masterTypes;
        }
    }
}