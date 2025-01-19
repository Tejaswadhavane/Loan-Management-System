using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using MVC_LoanSystem.Models;

namespace MVC_LoanSystem.Controllers
{
    public class CreatePlanMasterController : Controller
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;

        // GET: CreatePlanMaster
        public ActionResult Index()
        {
            try
            {
                var plans = GetPlansFromDatabase();
                return View(plans);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error retrieving data: " + ex.Message;
                return View(new List<loanSystem>());
            }
        }

        // POST: CreatePlanMaster/Create
        [HttpPost]
        public ActionResult Create(string PlanName, int Tenure, decimal RateOfInterest)
        {
            if (string.IsNullOrWhiteSpace(PlanName) || Tenure <= 0 || RateOfInterest <= 0)
            {
                TempData["ErrorMessage"] = "Please provide valid data.";
                return RedirectToAction("Index");
            }

            try
            {
                AddPlanToDatabase(new loanSystem
                {
                    planname = PlanName,
                    tenure = Tenure,
                    rateofinterest = RateOfInterest
                });

                TempData["Message"] = "Plan successfully created!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating plan: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Method to fetch plans from the database
        private List<loanSystem> GetPlansFromDatabase()
        {
            var plans = new List<loanSystem>();
            const string query = "SELECT PlanName, Tenure, RateOfInterest FROM CreatePlan";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        plans.Add(new loanSystem
                        {
                            planname = reader["PlanName"].ToString(),
                            tenure = Convert.ToInt32(reader["Tenure"]),
                            rateofinterest = Convert.ToDecimal(reader["RateOfInterest"])
                        });
                    }
                }
            }

            return plans;
        }

        // Method to add a plan to the database
        private void AddPlanToDatabase(loanSystem plan)
        {
            const string query = "INSERT INTO CreatePlan (PlanName, Tenure, RateOfInterest) VALUES (@PlanName, @Tenure, @RateOfInterest)";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@PlanName", plan.planname);
                cmd.Parameters.AddWithValue("@Tenure", plan.tenure);
                cmd.Parameters.AddWithValue("@RateOfInterest", plan.rateofinterest);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private void LoadPlansIntoDropDownList()
        {
            string query = "SELECT planname FROM CreatePlan";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Populate dropdown using planname as both the value and display
                ViewBag.Plans = new SelectList(dt.AsEnumerable(), "planname", "planname");
            }
        }

    }
}
