using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DailyProgressReport.Models;
using DailyProgressReport.Classes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.DirectoryServices.AccountManagement;
using System.Configuration;
using System.Reflection;
using System.Security.Policy;


namespace DailyProgressReport

{
    public class DprBOQReferenceController : Controller
    {
        private readonly IConfiguration _configuration;


        public DprBOQReferenceController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IActionResult Index()
        {
            try
            {
                string loggedInUserId = HttpContext.Session.GetString("SLoggedInUserID");

                // List<DprBOQReference> boqReference = GetBOQRE();
                List<ActivityModel> activity = GetActivityFromDatabase();
                ViewBag.ProjectsData = new SelectList(activity, "Id", "ActivityCode");
                List<GroupCodeModel> group = GetGroupFromDatabase();
                ViewBag.ProjectsData = new SelectList(group, "ID", "GroupCode");
                ShowBoqReferences model = new ShowBoqReferences();
                List<DprBOQHead> boqHeads = GetBOQHeadListFromDatabase(loggedInUserId);
                List<DprBOQReference> boqReferences = GetBOQReferenceListFromDatabase(loggedInUserId);
                // List<DprBOQReference> kor = GetActivity();
                model.BoqHeads = boqHeads;
                model.BoqReferences = boqReferences;
                model.Activity = activity;
                model.Group = group;
                //model.BoqReferences = kor;


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
        private List<ActivityModel> GetActivityFromDatabase()
        {
            List<ActivityModel> activity = new List<ActivityModel>();


            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();


                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tbl_ActivityCode ORDER BY Id DESC", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ActivityModel project = new ActivityModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ActivityCode = reader["ActivityCode"].ToString(),
                                // Add other properties as needed
                            };


                            activity.Add(project);
                        }
                    }
                }
            }


            return activity;
        }


        private List<GroupCodeModel> GetGroupFromDatabase()
        {
            List<GroupCodeModel> projects = new List<GroupCodeModel>();


            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();


                using (SqlCommand command = new SqlCommand("SELECT * FROM dpr_tbl_GroupCode ORDER BY Id DESC", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GroupCodeModel project = new GroupCodeModel
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                GroupCode = reader["GroupCode"].ToString(),
                                // Add other properties as needed
                            };


                            projects.Add(project);
                        }
                    }
                }
            }


            return projects;
        }


        private List<DprBOQHead> GetBOQHeadListFromDatabase(string loggedInUserId)
        {
            List<DprBOQHead> boqHeads = new List<DprBOQHead>();
            CommonFunction cmn = new CommonFunction();


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


        //[HttpGet]
        //public IActionResult GetBOQReferenceList()
        //{
        //    try
        //    {
        //        List<DprBOQReference> boqReferences = GetBOQReferenceListFromDatabase();
        //        return Json(new { success = true, boqReferences });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = $"Error fetching BOQ references: {ex.Message}" });
        //    }
        //}

     
        [HttpGet]
        public IActionResult GetBOQReferenceById(int id)
        {
            try
            {
                DprBOQReference boqReference = GetBOQReferenceByIdFromDatabase(id);
                return Json(new { success = true, boqReference });
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
        public IActionResult AddBOQReference(string boqHead, string boqNo, string wbsNumber, string boqDescription, string uom, string groupCode, string activityCode, string designQuantity)
        {
            try
            {
                string createdBy = HttpContext.Session.GetString("SLoggedInUserID");
                AddBOQReferenceToDatabase(boqHead, boqNo, wbsNumber, boqDescription, uom, groupCode, activityCode, designQuantity, createdBy);
                return Json(new { success = true, message = "BOQ Reference added successfully!" });
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
        public IActionResult UpdateBOQReference(int id, string boqHead, string boqNo, string wbsNumber, string boqDescription, string uom, string groupCode, string activityCode, string designQuantity)
        {
            try
            {
                string updatedBy = HttpContext.Session.GetString("SLoggedInUserID");
                UpdateBOQReferenceInDatabase(id, boqHead, boqNo, wbsNumber, boqDescription, uom, groupCode, activityCode, designQuantity, updatedBy);
                return Json(new { success = true, message = "BOQ Reference updated successfully!" });
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

        public IActionResult DeleteBOQReference(int id)
        {
            try
            {
                DeleteBOQReferenceFromDatabase(id);
                return Json(new { success = true, message = "BOQ Reference deleted successfully!" });
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


        private List<DprBOQReference> GetBOQReferenceListFromDatabase(string loggedInUserId)
        {
            List<DprBOQReference> boqReferences = new List<DprBOQReference>();
            CommonFunction cmn = new CommonFunction();


            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();


                using (SqlCommand command = new SqlCommand("dpr_sp_GetBOQReferenceList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", loggedInUserId);


                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DprBOQReference boqReference = new DprBOQReference
                            {
                                Id = (int)reader["Id"],
                                BOQHead = reader["BOQHead"].ToString(),
                                BOQNo = reader["BOQNo"].ToString(),
                                WBSNumber = reader["WBSNumber"].ToString(),
                                BOQDescription = reader["BOQDescription"].ToString(),
                                UOM = reader["UOM"].ToString(),
                                //    Length = reader["Length"].ToString(),
                                GroupCode = reader["GroupCode"].ToString(),
                                ActivityCode = reader["ActivityCode"].ToString(),
                                DesignQuantity = reader["BlockQuantity"].ToString(),
                                CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                            };


                            boqReferences.Add(boqReference);
                        }
                    }
                }
            }


            return boqReferences;
        }


        private DprBOQReference GetBOQReferenceByIdFromDatabase(int id)
        {
            DprBOQReference boqReference = null;
            CommonFunction cmn = new CommonFunction();
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();


                    using (SqlCommand command = new SqlCommand("dpr_sp_GetBOQReferenceById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Id", id);


                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                boqReference = new DprBOQReference
                                {
                                    Id = (int)reader["Id"],
                                    BOQHead = reader["BOQHead"].ToString(),
                                    BOQNo = reader["BOQNo"].ToString(),
                                    WBSNumber = reader["WBSNumber"].ToString(),
                                    BOQDescription = reader["BOQDescription"].ToString(),
                                    UOM = reader["UOM"].ToString(),
                                    //   Length = reader["Length"].ToString(),
                                    GroupCode = reader["GroupCode"].ToString(),
                                    ActivityCode = reader["ActivityCode"].ToString(),
                                    DesignQuantity = reader["BlockQuantity"].ToString(),
                                    CreatedDate = cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["CreatedDate"].ToString()),
                                    CreatedBy = reader["CreatedBy"].ToString(),
                                    UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? cmn.ConvertDateFormatmmddyytoddmmyyDuringDisplay(reader["UpdatedDate"].ToString()) : "",
                                    UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
                                };
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {


                throw;
            }


            return boqReference;
        }


        private void AddBOQReferenceToDatabase(string boqHead, string boqNo, string wbsNumber, string boqDescription, string uom, string groupCode, string activityCode, string designQuantity, string createdBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_sp_AddBOQReference", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@BOQHead", boqHead);
                    command.Parameters.AddWithValue("@BOQNo", boqNo);
                    command.Parameters.AddWithValue("@WBSNumber", wbsNumber);
                    command.Parameters.AddWithValue("@BOQDescription", boqDescription);
                    command.Parameters.AddWithValue("@UOM", uom);
                    //  command.Parameters.AddWithValue("@Length", DBNull.Value); // Since Length is not used in the stored procedure
                    command.Parameters.AddWithValue("@GroupCode", Convert.ToInt32(groupCode));
                    command.Parameters.AddWithValue("@ActivityCode", Convert.ToInt32(activityCode));
                    command.Parameters.AddWithValue("@DesignQuantity", designQuantity);
                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    command.ExecuteNonQuery();
                }
            }
        }


        private void UpdateBOQReferenceInDatabase(int id, string boqHead, string boqNo, string wbsNumber, string boqDescription, string uom, string groupCode, string activityCode, string designQuantity, string updatedBy)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();


                using (SqlCommand command = new SqlCommand("dpr_sp_UpdateBOQReference", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@BOQHead", boqHead);
                    command.Parameters.AddWithValue("@BOQNo", boqNo);
                    command.Parameters.AddWithValue("@WBSNumber", wbsNumber);
                    command.Parameters.AddWithValue("@BOQDescription", boqDescription);
                    command.Parameters.AddWithValue("@UOM", uom);
                    //   command.Parameters.AddWithValue("@Length", length);
                    command.Parameters.AddWithValue("@GroupCode", groupCode);
                    command.Parameters.AddWithValue("@ActivityCode", activityCode);
                    command.Parameters.AddWithValue("@DesignQuantity", designQuantity);
                    command.Parameters.AddWithValue("@UpdatedBy", updatedBy);


                    command.ExecuteNonQuery();
                }
            }
        }


        private void DeleteBOQReferenceFromDatabase(int id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();


                using (SqlCommand command = new SqlCommand("dpr_sp_DeleteBOQReference", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);


                    command.ExecuteNonQuery();
                }
            }
        }
    }





}