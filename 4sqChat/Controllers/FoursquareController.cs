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
            if (User.Identity.IsAuthenticated)
                return Redirect("/Profile?targetID=" + User.Identity.Name);
            return View();
        }

        public ActionResult NearbyVenues()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            string token = GetCurrentUserToken();
            logger.Debug("Got token "+token);
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(token);
            //TODO add parameters
            List<string> res = FSQOAuth.GetNearbyVenues(1000);
            if (res == null)
            {
                ViewBag.venues = null;
                return View();
            }

            logger.Debug("Got venues " + res.Count);
            List<FoursquareOAuth.Venue> venues = new List<FoursquareOAuth.Venue>();
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

        public ActionResult RandomChat()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            String token = GetCurrentUserToken();
            FoursquareOAuth foursquareOAuth = new FoursquareOAuth(token);
            List<int> users = foursquareOAuth.GetNearByUsers();
            Random random = new Random();
            int pos = random.Next(0, users.Count);
            return RedirectToAction("Chat", new {id = users[pos]});
        }
        
        public ActionResult Chat(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            ViewBag.to = id;
            FoursquareOAuth foursquareOAuth = new FoursquareOAuth(GetCurrentUserToken());
            bool isFriend = foursquareOAuth.CheckForFriendship(id);
            ViewBag.isFriend = isFriend;
            ViewBag.token = GetCurrentUserToken();
            return View();
        }

        public ActionResult ChatList()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "FoursquareLogin");
            IMessageRepository repository = new MessageRepository(new FoursquareUserContext());
            int uId = Convert.ToInt32(User.Identity.Name);
            IEnumerable<MessageModel> messages =
                repository.GetMessages().Where(model => model.From == uId || model.To == uId);
            List<int> res = new List<int>();
            foreach (MessageModel messageModel in messages)
            {
                if (messageModel.From != uId)
                    res.Add(messageModel.From);
                else
                    res.Add(messageModel.To);
            }
            ViewBag.Chats = res.Distinct();
            List<string> names = new List<string>();
            foreach (int id in ViewBag.Chats)
            {
                NameValueCollection tmp;

                tmp = GetProfileInfo(id);
                names.Add(tmp["firstname"]);
            }
            ViewBag.Names = names;
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
