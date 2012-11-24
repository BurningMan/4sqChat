using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;



namespace _4sqChat.Controllers
{
    public class FoursquareLoginController : Controller
    {
        //
        // GET: /FoursquareLogin/
        public Logic.Foursquare_oAuth FSQOAuth;
        

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            
            return View();
        }

        public ActionResult Authenticate()
        {
            if (FSQOAuth == null)
                FSQOAuth = new Logic.Foursquare_oAuth(ConfigurationManager.AppSettings["FSQClientID"],
                    ConfigurationManager.AppSettings["FSQClientSecret"],
                    ConfigurationManager.AppSettings["FSQCallback"]);
            if (Request.QueryString["code"] == null)
            {
                return Redirect(FSQOAuth.GetAuthURL());
            }
            else
            {
                if (FSQOAuth.makeAuthentication(Request["code"]))
                {
                    ViewBag.suc = true;

                    ViewBag.userId = FSQOAuth.GetUserId();
                }
                else
                    ViewBag.suc = false;
            }
            return View();

        }
        

    }

    
}
