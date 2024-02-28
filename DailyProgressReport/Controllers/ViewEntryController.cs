using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DailyProgressReport.Models;
using System.Data;
using Microsoft.Extensions.Configuration;
using DailyProgressReport.Classes;

namespace DailyProgressReport.Controllers

{

    public class ViewEntryController : Controller
    {
        // Replace the connection string with your actual connection string
        //private readonly string ConnectionString = "Data Source=SG2NWPLS19SQL-v04.mssql.shr.prod.sin2.secureserver.net;Initial Catalog=SandboxDB;User ID=rockpock;Password=v6*w1kO33\"";
        private readonly IConfiguration _configuration; // Inject your configuration if needed

        public ViewEntryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            // try
            // {
            ShowViewEntry kor = new ShowViewEntry();

            ErrorViewModel error = new ErrorViewModel();

            var viewEntryViewModels = GetMaterialTransactions();

            List<ViewEntryViewModel> activities = GetMaterialTransactions();
            kor.viewentry = activities;
            return View(kor);
            // }
            //catch (Exception ex)
            //{
            ////    // Log the exception
            //    return View("Error");
            //}
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var entryToEdit = GetMaterialTransactionsById(id);
                return View(entryToEdit);
            }
            catch (Exception ex)
            {
                // Log the exception
                return View("Error");
            }
        }
        [HttpPost]
        public IActionResult Edit(ViewEntryViewModel updatedEntry)
        {
            try
            {
                if (updatedEntry.Date == null)
                {
                    // Handle the case where Date is null (e.g., set it to DateTime.Now)
                    updatedEntry.Date = DateTime.MaxValue;
                }
                UpdateMaterialTransaction(updatedEntry);

                // Handle the redirect logic here
                return RedirectToAction("Index", new { id = updatedEntry.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating entry: " + ex.Message });
            }
        }

        [HttpPost]

        public IActionResult CancelEdit(int id)
        {
            try
            {
                // Get original entry details from the database and pass it to the view
                var originalEntry = GetMaterialTransactionsById(id);
                return View("Index", originalEntry);
            }
            catch (Exception ex)
            {
                // Log the exception
                return View("Error");
            }
        }


        [HttpPost]
        public IActionResult Submit(int id)
        {
            try
            {
                UpdateIsSubmitted(id);
                return Json(new { success = true, message = "Entry submitted successfully.", id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error submitting entry: " + ex.Message });
            }
        }




        public IActionResult Delete(int id)
        {
            try
            {
                // Perform deletion operation based on the provided JobCode
                DeleteMaterialTransaction(id);

                // Redirect back to the Index action after deletion
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during deletion
                // You can log the error or show an error message to the user
                return RedirectToAction("Index"); // or return an error view
            }
        }




        private ViewEntryViewModel GetMaterialTransactionsById(int id)
        {
            // Implement your logic to retrieve a single entry by Id
            // For simplicity, you can use the existing GetMaterialTransactions method
            return GetMaterialTransactions().FirstOrDefault(entry => entry.Id == id);
        }


        [HttpPost]
        public IActionResult Edit(List<ViewEntryViewModel> updatedEntries)
        {
            try
            {
                // Process and update the entry in the database
                UpdateMaterialTransaction(updatedEntries);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating entry: " + ex.Message });
            }
        }

        private void UpdateMaterialTransaction(ViewEntryViewModel model)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateMaterialTransaction", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@ID", model.Id);
                    //command.Parameters.AddWithValue("@ProjectName", model.ProjectName);

                    //command.Parameters.AddWithValue("@BlockName", model.BlockName);
                    //command.Parameters.AddWithValue("@ComponentName", model.ComponentName);
                    //command.Parameters.AddWithValue("@LocationName", model.LocationName);
                    //command.Parameters.AddWithValue("@VillageName", model.VillageName);
                    //command.Parameters.AddWithValue("@BOQHeadName", model.BOQHeadName);
                    //command.Parameters.AddWithValue("@BOQReferenceID", model.BOQReferenceID);
                    //command.Parameters.AddWithValue("@ActivityCode", model.ActivityCode);
                    command.Parameters.AddWithValue("@BlockQuantity", model.BlockQuantity); // Assuming BlockQuantity is a property of type int in ViewEntryViewModel

                    command.Parameters.AddWithValue("@TypeOfPipe", model.TypeOfPipe);
                    command.Parameters.AddWithValue("@DiaOfPipe", model.DiaOfPipe);
                    command.Parameters.AddWithValue("@UOM", model.UOM);
                    command.Parameters.AddWithValue("@JTDQuantity", model.JTDQuantity);
                    command.Parameters.AddWithValue("@DayQuantity", model.DayQuantity);
                    command.Parameters.AddWithValue("@IsSubmitted", model.IsSubmitted);
                    command.Parameters.AddWithValue("@WBSNumber", model.WBSNumber);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void UpdateMaterialTransaction(List<ViewEntryViewModel> models)
        {
            foreach (var model in models)
            {
                UpdateMaterialTransaction(model);
            }
        }



        private void UpdateIsSubmitted(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();
                string query = "UPDATE dpr_MaterialTransactions SET IsSubmitted = 1 WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private List<ViewEntryViewModel> GetMaterialTransactions()
        {
            var activities = new List<ViewEntryViewModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_GetMaterialTransactions", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var viewentryViewModels = new ViewEntryViewModel
                            {
                                Id = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                                // Date = (DateOnly)reader["Date"],
                                Date = reader["Date"] != DBNull.Value ? (DateTime)reader["Date"] : DateTime.MinValue,
                                //Date = reader["Date"] != cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["Date"].ToString()),
                                //Date = (DateTime)reader["Date"],

                                ProjectName = reader["ProjectName"] as string ?? string.Empty,
                                BlockName = reader["BlockName"] as string ?? string.Empty,
                                ComponentName = reader["ComponentName"] as string ?? string.Empty,
                                LocationName = reader["LocationName"] as string ?? string.Empty,
                                VillageName = reader["VillageName"] as string ?? string.Empty,
                                BOQHeadName = reader["BOQHeadName"] as string ?? string.Empty,
                                BOQReferenceID = reader["BOQReferenceID"] as string ?? string.Empty,
                                ActivityCode = reader["ActivityCode"] as string ?? string.Empty,
                                BlockQuantity = reader["BlockQuantity"] != DBNull.Value ? (int)reader["BlockQuantity"] : (int?)null,
                                TypeOfPipe = reader["TypeOfPipe"] as string ?? string.Empty,
                                DiaOfPipe = reader["DiaOfPipe"] as string ?? string.Empty,
                                UOM = reader["UOM"] as string ?? string.Empty,
                                JTDQuantity = reader["JTDQuantity"] != DBNull.Value ? (int)reader["JTDQuantity"] : (int?)null,
                                DayQuantity = reader["DayQuantity"] != DBNull.Value ? (int)reader["DayQuantity"] : (int?)null,
                                IsSubmitted = reader["IsSubmitted"] != DBNull.Value && (bool)reader["IsSubmitted"],
                                WBSNumber = reader["WBSNumber"] as string ?? string.Empty
                            };

                            activities.Add(viewentryViewModels);
                        }
                    }
                }
            }

            return activities;
        }






        //private void DeleteMaterialTransaction(int id)
        //{
        //    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
        //    {
        //        connection.Open();

        //        string query = "DELETE FROM dpr_MaterialTransactions WHERE Id = @Id";

        //        using (SqlCommand command = new SqlCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("@Id", id);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        [HttpPost]
        public ActionResult DeleteMaterialTransaction(int Id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    string sqlQuery = "DELETE FROM dpr_MaterialTransactions WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "View Entry deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting View Entry", error = ex.Message });
            }
        }






        [HttpPost]
        public IActionResult UpdateViewEntry(int id, int BlockQuantity, int JTDQuantity, int DayQuantity)
        {
            try
            {
                string updatedBy = HttpContext.Session.GetString("SLoggedInUserID");
                UpdateViewEntryInDatabase(id, BlockQuantity, JTDQuantity, DayQuantity);
                return Json(new { success = true, message = "View Entry  updated successfully!" });
            }
            catch (Exception ex)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string exceptionMessage = $"Exception in method '{methodName}'";


                // Log or rethrow the exception with the updated message
                //  var errorLogger = new CustomErrorLog(_configuration);
                //  errorLogger.LogError(ex, exceptionMessage);
                return Json(new { error = ex.Message });
            }
        }


        private void UpdateViewEntryInDatabase(int id, int BlockQuantity, int JTDQuantity, int DayQuantity)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();


                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateMaterialTransaction", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@BlockQuantity", BlockQuantity); // Assuming BlockQuantity is a property of type int in ViewEntryViewModel

                    command.Parameters.AddWithValue("@JTDQuantity", JTDQuantity);
                    command.Parameters.AddWithValue("@DayQuantity", DayQuantity);
                    //command.Parameters.AddWithValue("@IsSubmitted",IsSubmitted);
                    //command.Parameters.AddWithValue("@WBSNumber", WBSNumber);


                    command.ExecuteNonQuery();
                }
            }
        }




        // Controller action responsible for handling the AJAX request
        [HttpPost]
        public ActionResult IsSubmittedUpdateMaterialTransaction(int id, bool isSubmitted)
        {
            try
            {
                // Call the stored procedure to update IsSubmitted in the database
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();
                    var command = new SqlCommand("dpr_sp_IsSubmittedUpdateMaterialTransaction", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@IsSubmitted", isSubmitted);
                    command.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




    }
}