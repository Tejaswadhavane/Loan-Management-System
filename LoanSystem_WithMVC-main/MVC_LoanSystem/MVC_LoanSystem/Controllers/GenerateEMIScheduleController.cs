using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using MVC_LoanSystem.Models;

namespace MVC_LoanSystem.Controllers
{
    public class GenerateEMIScheduleController : Controller
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;

        // GET: GenerateEMISchedule
        public ActionResult Index()
        {
            try
            {
                // Load plans into dropdown list
                LoadPlansIntoDropDownList();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error retrieving data: " + ex.Message;
                return View();
            }
        }

        // Method to load plans into dropdown list
        private void LoadPlansIntoDropDownList()
        {
            string query = "SELECT PlanID, PlanName FROM CreatePlan";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Convert DataTable to List<SelectListItem>
                var plans = dt.AsEnumerable()
                              .Select(row => new SelectListItem
                              {
                                  Value = row["PlanID"].ToString(),
                                  Text = row["PlanName"].ToString()
                              }).ToList();

                // Create a SelectList from the List<SelectListItem>
                ViewBag.Plans = new SelectList(plans, "Value", "Text");
            }
        }

        // POST: GenerateEMISchedule/GetPlanDetails
        [HttpPost]
        public JsonResult GetPlanDetails(int planId)
        {
            try
            {
                string query = "SELECT Tenure, RateOfInterest FROM CreatePlan WHERE PlanID = @PlanID";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PlanID", planId);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var planDetails = new
                            {
                                Tenure = reader["Tenure"],
                                RateOfInterest = reader["RateOfInterest"]
                            };
                            return Json(planDetails, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error retrieving plan details: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: GenerateEMISchedule/CalculateEMI
        [HttpPost]
        public JsonResult CalculateEMI(decimal loanAmount, int tenure, decimal rateOfInterest)
        {
            try
            {
                // EMI Calculation Formula
                decimal monthlyRate = rateOfInterest / 12 / 100;
                decimal emi = loanAmount * monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, tenure) / ((decimal)Math.Pow(1 + (double)monthlyRate, tenure) - 1);
                decimal totalEmi = emi * tenure;
                decimal extraAmount = totalEmi - loanAmount;

                var emiDetails = new
                {
                    Emi = emi.ToString("F2"),
                    TotalEmi = totalEmi.ToString("F2"),
                    ExtraAmount = extraAmount.ToString("F2")
                };

                return Json(emiDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error calculating EMI: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: GenerateEMISchedule/GenerateSchedule
        [HttpPost]
        public JsonResult GenerateSchedule(decimal loanAmount, decimal emiAmount, string loanDate, int tenure)
        {
            try
            {
                DateTime loanDateParsed = DateTime.Parse(loanDate);
                List<EmiSchedule> schedule = new List<EmiSchedule>();

                for (int i = 1; i <= tenure; i++)
                {
                    DateTime dueDate = loanDateParsed.AddMonths(i);
                    schedule.Add(new EmiSchedule
                    {
                        EmiNo = i,
                        DueDate = dueDate.ToString("yyyy-MM-dd"),
                        EmiAmount = emiAmount.ToString("F2")
                    });
                }

                return Json(new { Schedule = schedule }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error generating EMI schedule: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }

    // Model for EMI Schedule
    public class EmiSchedule
    {
        public int EmiNo { get; set; }
        public string DueDate { get; set; }
        public string EmiAmount { get; set; }
    }
}
