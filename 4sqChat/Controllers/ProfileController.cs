using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _4sqChat.Logic;
using _4sqChat.Models;

namespace _4sqChat.Controllers
{
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/
        public NameValueCollection GetProfileInfo(int targetId)
        {
            string token = GetCurrentUserToken(); 
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(token);
            Profile pf = FSQOAuth.GetProfileInfo(targetId);
            Models.FoursquareUserContext db = new FoursquareUserContext();
            int userID = FSQOAuth.GetUserId();
            FoursquareUserModel um = db.FoursquareUsers.Find(userID);
            NameValueCollection nv = pf.getInfo(true); 
            return nv;
        }
        private string GetCurrentUserToken()
        {
            if (!User.Identity.IsAuthenticated)
                return null;
            Models.FoursquareUserContext fsqDBContext = new FoursquareUserContext();
            Models.FoursquareUserModel um = fsqDBContext.FoursquareUsers.Find(Convert.ToInt32(User.Identity.Name));
            if (um != null)
                return um.Token;
            return null;

        }

        public ActionResult Index()
        {
            if (Request["targetID"] == null)
                return HttpNotFound();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            int targetId = Convert.ToInt32(Request["targetID"]);
            ViewBag.profile = GetProfileInfo(targetId);
            return View();
        }

    }
}
