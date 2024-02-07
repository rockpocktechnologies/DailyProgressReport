using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DailyProgressReport.Models;
using System.Data;
using Microsoft.Extensions.Configuration;


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
            try
            {
                var viewEntryViewModels = GetMaterialTransactions();
                return View(viewEntryViewModels);
            }
            catch (Exception ex)
            {
                // Log the exception
                return View("Error");
            }
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
                    updatedEntry.Date = DateTime.Now;
                }
                UpdateMaterialTransaction(updatedEntry);
                //return RedirectToAction("Index");
                return Json(new { success = true, message = "Entry updated successfully." });

            }
            catch (Exception ex)
            {
                // Log the exception
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

        public IActionResult Delete(int id)
        {
            try
            {
                DeleteMaterialTransaction(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                return View("Error");
            }
        }

        public IActionResult Submit(int id)
        {
            try
            {
                UpdateIsSubmitted(id);
                return Json(new { success = true, message = "Entry submitted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error submitting entry: " + ex.Message });
            }
        }
        //    // Process and update the entry in the database
        //    UpdateMaterialTransaction(updatedEntry);

        //    return RedirectToAction("Index");
        //}   



        //public IActionResult Delete(int id)
        //{
        //    // Perform deletion operation based on the provided id
        //    DeleteMaterialTransaction(id);

        //    // Redirect back to the Index action after deletion
        //    return RedirectToAction("Index");

        //}


        //public IActionResult Submit(int id)
        //{
        //    try
        //    {
        //        // Call a method to update IsSubmitted in the database
        //        UpdateIsSubmitted(id);
        //        return Json(new { success = true, message = "Entry submitted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return an error message if an exception occurs
        //        return Json(new { success = false, message = "Error submitting entry: " + ex.Message });
        //    }
        //}



        private ViewEntryViewModel GetMaterialTransactionsById(int id)
        {
            // Implement your logic to retrieve a single entry by Id
            // For simplicity, you can use the existing GetMaterialTransactions method
            return GetMaterialTransactions().FirstOrDefault(entry => entry.Id == id);
        }


        [HttpPost]
        public IActionResult EditEntryes(List<ViewEntryViewModel> updatedEntries)
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

                string query = @"UPDATE dpr_MaterialTransactions 
                         SET Date = @Date, 
                             JobCodeID = @JobCodeID, 
                             BlockName = @BlockName, 
                             ComponentName = @ComponentName, 
                             LocationName = @LocationName, 
                             VillageName = @VillageName, 
                             BOQHeadName = @BOQHeadName,
                             BOQReferenceID = @BOQReferenceID, 
                             ActivityCode = @ActivityCode, 
                             BlockQuantity = @BlockQuantity, 
                             TypeOfPipe = @TypeOfPipe, 
                             DiaOfPipe = @DiaOfPipe, 
                             UOM = @UOM, 
                             JTDQuantity = @JTDQuantity, 
                             DayQuantity = @DayQuantity, 
                              
                             IsSubmitted = @IsSubmitted, 
                             WBSNumber = @WBSNumber 
                         FROM dpr_MaterialTransactions mt
                         LEFT JOIN dpr_block b ON mt.ID = b.Id
                         LEFT JOIN dpr_addcomponent c ON mt.ComponentID = c.Id
                         LEFT JOIN dpr_location l ON mt.LocationID = l.Id
                         LEFT JOIN dpr_village vn ON mt.VillageNameID = vn.Id
                         LEFT JOIN dpr_boqhead bh ON mt.BOQHeadID = bh.Id
                         LEFT JOIN dpr_tbl_ActivityCode a ON mt.ActivityID = a.Id
                         LEFT JOIN dpr_tblProjects p ON mt.Id = p.ProjectID
                       
                         WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.AddWithValue("@Id", model.Id);
                    command.Parameters.AddWithValue("@Date", model.Date);
                    command.Parameters.AddWithValue("@JobCodeID", model.JobCodeID);
                    command.Parameters.AddWithValue("@BlockName", model.BlockName);
                    command.Parameters.AddWithValue("@ComponentName", model.ComponentName);
                    command.Parameters.AddWithValue("@LocationName", model.LocationName);
                    command.Parameters.AddWithValue("@VillageName", model.VillageName);
                    command.Parameters.AddWithValue("@BOQHeadName", model.BOQHeadName);
                    command.Parameters.AddWithValue("@BOQReferenceID", model.BOQReferenceID);
                    command.Parameters.AddWithValue("@ActivityCode", model.ActivityCode);
                    command.Parameters.AddWithValue("@BlockQuantity", model.BlockQuantity);
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
            var viewentryViewModels = new List<ViewEntryViewModel>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                string query = @"SELECT mt.Date, mt.JobCodeID, b.BlockName, c.ComponentName, 
        l.LocationName, vn.VillageName, bh.BOQHeadName, mt.BOQReferenceID,
        a.ActivityCode, mt.BlockQuantity, mt.TypeOfPipe, mt.DiaOfPipe, mt.UOM, 
        mt.JTDQuantity, mt.DayQuantity, mt.IsSubmitted, mt.WBSNumber
        FROM rockpock.dpr_MaterialTransactions mt
                        
        LEFT JOIN dpr_block b ON mt.ID = b.Id
        LEFT JOIN dpr_addcomponent c ON mt.ComponentID = c.Id
        LEFT JOIN dpr_location l ON mt.LocationID = l.Id
        LEFT JOIN dpr_village vn ON mt.VillageNameID = vn.Id
        LEFT JOIN dpr_boqhead bh ON mt.BOQHeadID = bh.Id
        LEFT JOIN dpr_tbl_ActivityCode a ON mt.ActivityID = a.Id";


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var viewentryViewModel = new ViewEntryViewModel
                            {
                                //Id = Convert.ToInt32(reader["Id"]),
                                Date = reader["Date"] as DateTime?,
                                JobCodeID = reader["JobCodeID"] as string,
                                BlockName = reader["BlockName"] as string,
                                ComponentName = reader["ComponentName"] as string,
                                LocationName = reader["LocationName"] as string,
                                VillageName = reader["VillageName"] as string,
                                BOQHeadName = reader["BOQHeadName"] as string,
                                BOQReferenceID = Convert.ToInt32(reader["BOQReferenceID"]),
                                ActivityCode = reader["ActivityCode"] as string,
                                BlockQuantity = reader["BlockQuantity"] as int?,
                                TypeOfPipe = reader["TypeOfPipe"] as string,
                                DiaOfPipe = reader["DiaOfPipe"] as string,
                                UOM = reader["UOM"] as string,
                                JTDQuantity = reader["JTDQuantity"] as int?,
                                DayQuantity = reader["DayQuantity"] as int?,
                                //Username = reader["username"] as string,
                                //CreatedDate = reader["CreatedDate"] as DateTime?,
                                //CreatedBy = reader["CreatedBy"] as string,
                                //ModifiedDate = reader["ModifiedDate"] as DateTime?,
                                //ModifiedBy = reader["ModifiedBy"] as string,
                                IsSubmitted = reader["issubmitted"] as bool?,
                                WBSNumber = reader["WBSNumber"] as string
                            };

                            viewentryViewModels.Add(viewentryViewModel);
                        }
                    }
                }
            }

            return viewentryViewModels;
        }

        private void DeleteMaterialTransaction(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                string query = "DELETE FROM rockpock.dpr_MaterialTransactions WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }


    }
}
