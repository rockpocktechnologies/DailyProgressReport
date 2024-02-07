using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using DailyProgressReport.Classes;
using DailyProgressReport.Models;

namespace DailyProgressReport.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /Login
        public IActionResult Index()
        {
            try
            {
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


        [HttpPost]
        public JsonResult ValidateLogin(string userId, string password)
        {
            try
            {

                //using (var context = new PrincipalContext(ContextType.Domain, "afconsinfra"))
                //{
                // Note: ValidateCredentials will throw an exception if authentication fails
                //if (context.ValidateCredentials(userId, password))
                //{
                // Check if the user is the super admin based on appsettings
                string superAdminUsername = _configuration["AppSettings:SuperAdminUsername"];
                string PlanningManagerUsername = _configuration["AppSettings:PlanningManager"];

                // string Admin = _configuration["AppSettings:admin"];
                //if (ValidationLogic(userId, password))//LDAP validation
                //{
                //    return Json(new { success = false, errorMessage = "Invalid credentials" });
                //}


                //HttpContext.Session.SetString("SLoggedInUserID", userId);

                if (userId == superAdminUsername)
                {
                    HttpContext.Session.SetString("SLoggedInUserID", userId);
                    HttpContext.Session.SetString("UserRole", "SuperAdmin");
                    // Super admin, redirect to Project/Index
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Project") });
                }
                else if(userId== "PlanningManager")
                {
                    HttpContext.Session.SetString("SLoggedInUserID", userId);
                    HttpContext.Session.SetString("UserRole", "PlanningManager");
                    // Planning Manager, redirect to PlanningManager/Index (customize as needed)
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "PlanningManager") });

                }
                else
                {
                    HttpContext.Session.SetString("SLoggedInUserID", userId);

                    HttpContext.Session.SetString("UserRole", "Admin");
                    // Admin, redirect to ProjectList/Index (customize as needed)
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "ProjectList") });

                }
                //else
                //{
                //    HttpContext.Session.SetString("SLoggedInUserID", userId);

                //    HttpContext.Session.SetString("UserRole", "PlanningManager");
                //    // Admin, redirect to ProjectList/Index (customize as needed)
                //    //return Json(new { success = true, redirectUrl = Url.Action("Index", "ProjectList") });
                //    return Json(new { success = true, redirectUrl = Url.Action("Index", "PlanningManager") });
                //}

                //// If not super admin, check if the user is an admin using the stored procedure
                //int adminId = CheckAdminId(userId);

                //if (adminId > 0)
                //{
                //    HttpContext.Session.SetString("UserRole", "Admin");
                //    // Admin, redirect to ProjectList/Index (customize as needed)
                //    return Json(new { success = true, redirectUrl = Url.Action("Index", "ProjectList") });
                //}
                //else
                //{

                //    // Regular user, handle accordingly
                //    return Json(new { success = false, errorMessage = "No projects assigned to you" });
                //}
                //}
                //else
                //{

                //    // Regular user, handle accordingly
                //    return Json(new { success = false, errorMessage = "Invalid credentials" });
                //}

                //}

                using (var context = new
                    PrincipalContext(ContextType.Domain, "afconsinfra"))
                {
                    // Note: ValidateCredentials will throw an exception if authentication fails
                    if (context.ValidateCredentials(userId, password))
                    {
                        // Check if the user is the super admin based on appsettings
                        //string superAdminUsername = _configuration["AppSettings:SuperAdminUsername"];

                        //if (ValidationLogic(userId, password))//LDAP validation
                        //{
                        //    return Json(new { success = false, errorMessage = "Invalid credentials" });
                        //}


                        HttpContext.Session.SetString("SLoggedInUserID", userId);

                        //if (userId == superAdminUsername)
                        //{
                        //    HttpContext.Session.SetString("UserRole", "SuperAdmin");
                        //    // Super admin, redirect to Project/Index
                        //    return Json(new { success = true, redirectUrl = Url.Action("Index", "Project") });
                        //}

                        // If not super admin, check if the user is an admin using the stored procedure
                        int adminId = CheckAdminId(userId);

                        if (adminId > 0)
                        {
                            HttpContext.Session.SetString("UserRole", "Admin");
                            // Admin, redirect to ProjectList/Index (customize as needed)
                            return Json(new { success = true, redirectUrl = Url.Action("Index", "ProjectList") });
                        }
                        else
                        {

                            // Regular user, handle accordingly
                            return Json(new { success = false, errorMessage = "No projects assigned to you" });
                        }
                    }
                    else
                    {

                        // Regular user, handle accordingly
                        return Json(new { success = false, errorMessage = "Invalid credentials" });
                    }

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

        //[HttpPost]
        //public JsonResult ValidateLogin(string userId, string password)
        //{
        //    // Perform your validation logic here
        //    if (YourValidationLogic(userId, password))
        //    {
        //        // Valid credentials, check if the user is an admin
        //        var isAdminResult = CheckAdminId(userId);

        //        if (isAdminResult.IsSuperAdmin)
        //        {
        //            // Super admin, redirect to Project/Index
        //            return Json(new { success = true, redirectUrl = Url.Action("Index", "Project") });
        //        }
        //        else if (isAdminResult.AdminId > 0)
        //        {
        //            // Admin, redirect to ProjectList/Index (customize as needed)
        //            return Json(new { success = true, redirectUrl = Url.Action("Index", "ProjectList") });
        //        }
        //        else
        //        {
        //            // Regular user, handle accordingly
        //            return Json(new { success = false, errorMessage = "Invalid credentials" });
        //        }
        //    }
        //    else
        //    {
        //        // Invalid credentials, return an error message
        //        return Json(new { success = false, errorMessage = "Invalid credentials" });
        //    }
        //}

        private bool ValidationLogic(string userId, string password)
        {
            // Implement your validation logic (e.g., check against a database)
            // Replace this with your actual validation logic
            return userId == "superadmin" && password == "yourPassword";
        }

        private int CheckAdminId(string username)
        {
            // Your ADO.NET code to call the stored procedure dpr_SPCheckAdminId
            // and return the AdminId or 0 if not found.

            // Example code:
            int adminId = 0;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dpr_SPCheckAdminId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", username);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            adminId = Convert.ToInt32(reader["AdminId"]);
                        }
                    }
                }
            }

            return adminId;
        }
    }
}





