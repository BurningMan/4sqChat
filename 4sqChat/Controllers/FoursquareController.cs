using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using _4sqChat.Logic;
using _4sqChat.Models;
using log4net;

namespace _4sqChat.Controllers
{
    public class FoursquareController : Controller
    {
        //
        // GET: /Foursquare/
        ILog logger = LogManager.GetLogger(typeof(FoursquareController));

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
            logger.Debug("Got token "+token);
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(token);

            List<string> res = FSQOAuth.GetNearbyVenues();
            if (res == null)
            {
                ViewBag.venues = null;
                return View();
            }

            logger.Debug("Got venues " + res.Count);
            List<NameValueCollection> venues = new List<NameValueCollection>();
            if (res.Count == 0)
            {
                ViewBag.venues = null;
                return View();
            }
            try
            {
                foreach (var re in res)
                {
                    venues.Add(FSQOAuth.GetVenuesInfo(re));
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            logger.Debug("Got venues info");
            ViewBag.venues = venues;
            return View();

        }

        public ActionResult NearbyUsers()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            string token = GetCurrentUserToken();
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(token);
            Models.FoursquareUserContext db = new FoursquareUserContext();
            IEnumerable<FoursquareUserModel> foursquareUsers = db.FoursquareUsers;
            foreach (var foursquareUserModel in foursquareUsers)
            {
                
                Logic.FoursquareOAuth tmp = new FoursquareOAuth(foursquareUserModel.Token);
                foursquareUserModel.LastVenueID = tmp.GetLastVenue();
                UpdateModel(foursquareUserModel);
                
                
                logger.Debug("Got last venue "+foursquareUserModel.FoursquareUserId);
            }
            db.SaveChanges();
            logger.Debug("Got all venues");
            List<int> res = FSQOAuth.GetNearByUsers();
            logger.Debug("got nearby users");
            List<string> names= new List<string>();
            for (int i = 0; i < res.Count; ++i)
            {
                NameValueCollection tmp;
                if (res[i] != null)
                {
                    tmp = GetProfileInfo(res[i]);
                    names.Add(tmp["firstname"]);
                }
                else
                {
                    res.Remove(i);
                }
            }
            logger.Debug("got profile info");
            ViewBag.users = res;
            ViewBag.names = names;
            return View();
        }
        public NameValueCollection GetProfileInfo(int targetId)
        {
            string token = GetCurrentUserToken();
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(token);
            Profile pf = FSQOAuth.GetProfileInfo(targetId);
            Models.FoursquareUserContext db = new FoursquareUserContext();
            int userID = FSQOAuth.GetUserId();
            FoursquareUserModel um = db.FoursquareUsers.Find(userID);
            NameValueCollection nv = pf.getInfo(um.IsPremium);
            return nv;
        }
        
        public ActionResult Chat(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            ViewBag.to = id;
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
