using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_LoanSystem.Models;

namespace MVC_LoanSystem.Controllers
{
    public class HomeController : Controller
    {
        public readonly String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        
        // GET: CreatePlanMaster
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult CreatePlanMaster()
        {
            // Using ViewBag to store temporary data for the current request
            ViewBag.Message = "Plan Master Creation in Progress";

            // Using Session to store data for the user's session
            Session["PlanDetails"] = "PlanName: LoanPlan1, Duration: 12 Months";

            // Using a Cookie to store some user-specific data
            HttpCookie userCookie = new HttpCookie("UserPreference");
            userCookie["Theme"] = "Dark";
            userCookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(userCookie);

            // Redirecting the user to CreatePlanMaster action in the CreatePlanMasterController
            return RedirectToAction("Index", "CreatePlanMaster");
        }

        public ActionResult GenerateEMISchedule()
        {


            //return View();
            return RedirectToAction("Index", "GenerateEMISchedule");

        }
    }
}