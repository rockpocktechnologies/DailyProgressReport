using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;



namespace DailyProgressReport.Controllers
{
    // ProjectSiteEnggAssignmentController.cs
    [ApiController]
    [Route("api/[controller]")]

    public class ProjectSiteEnggAssignmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProjectSiteEnggAssignmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetJobs")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs(string siteEnggId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("ConnectionString")))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("GetJobsBySiteEnggID", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@SiteEnggID", siteEnggId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<Job> jobs = new List<Job>();

                            while (reader.Read())
                            {
                                Job job = new Job
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    // Add other properties as needed
                                };

                                jobs.Add(job);
                            }

                            return jobs;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        public class Job
        {
            public int Id { get; set; }
            public string Title { get; set; }
            // Add more properties as needed

            public override string ToString()
            {
                return Title; // Or format it as needed for display
            }
        }
    }

}
