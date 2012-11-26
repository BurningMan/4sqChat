using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using _4sqChat.Logic;
using _4sqChat.Models;

namespace _4sqChat.Controllers
{
    public class FoursquareController : Controller
    {
        //
        // GET: /Foursquare/

        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
                return View();
            return RedirectToAction("Authenticate", "FoursquareLogin");
        }

        public ActionResult NearbyVenues()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            string token = GetCurrentUserToken();
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(token);
            List<string> res = FSQOAuth.GetNearbyVenues();
            List<NameValueCollection> venues = new List<NameValueCollection>();
            if (res == null)
            {
                ViewBag.venues = null;
                return View();
            }
            foreach (var re in res)
            {
                venues.Add(FSQOAuth.GetVenuesInfo(re));
            }
            ViewBag.venues = venues;
            return View();

        }
        public ActionResult NearbyUsers()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            string token = GetCurrentUserToken();
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(token);
            List<int> res = FSQOAuth.GetNearByUsers();
            ViewBag.users = res;
            return View();
        }
        private string  GetCurrentUserToken()
        {
            if (!User.Identity.IsAuthenticated)
                return null;
            Models.FoursquareUserContext fsqDBContext = new FoursquareUserContext();
            Models.FoursquareUserModel um = fsqDBContext.FoursquareUsers.Find(Convert.ToInt32(User.Identity.Name));
            if (um != null)
                return um.Token;
            return null;

        }


    }
}
