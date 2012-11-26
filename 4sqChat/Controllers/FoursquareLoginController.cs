using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Web.Security;
using _4sqChat.Logic;
using _4sqChat.Models;


namespace _4sqChat.Controllers
{
    public class FoursquareLoginController : Controller
    {
        //
        // GET: /FoursquareLogin/

        public ActionResult Index()
        {
            Models.FoursquareUserContext fsqDBContext = new FoursquareUserContext();
            fsqDBContext.FoursquareUsers.Add(new FoursquareUserModel()
                {
                    FoursquareUserId = 1,
                    Token = "fafaf",
                    UserGuid = Guid.NewGuid(),
                    UserName = "1"
                });
            
            fsqDBContext.Messages.Add(new MessageModel()
                {
                    From = 1,
                    To = 1,
                    Message = "daf",
                    time = DateTime.Now
                });
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Authenticate");
            Models.FoursquareUserContext fsqDBContext = new FoursquareUserContext();
            Models.FoursquareUserModel fsqUser = fsqDBContext.FoursquareUsers.Find(Convert.ToInt32(User.Identity.Name));
            Logic.FoursquareOAuth FSQOAuth = new FoursquareOAuth(fsqUser.Token);
            try
            {
                FSQOAuth.GetUserId();
                return RedirectToAction("Index", "Foursquare");
            }
            catch (WebException)
            {
                return RedirectToAction("Authenticate");
            }
        }

        public ActionResult Authenticate()
        {

            Logic.FoursquareOAuth FSQOAuth = new Logic.FoursquareOAuth(null);
            if (Request["code"] == null)
            {
                return Redirect(FSQOAuth.GetAuthURL());
            }
            if (FSQOAuth.makeAuthentication(Request["code"]))
            {
                int userId = FSQOAuth.GetUserId();
                string lastVenue = FSQOAuth.GetLastVenue();
                FoursquareUserContext fsqDBContext = new FoursquareUserContext();
                FoursquareUserModel curUser = fsqDBContext.FoursquareUsers.Find(userId);
                if (curUser != null)
                {
                    MembershipUser mUser = Membership.GetUser(curUser.UserGuid);
                    curUser.LastVenueID = lastVenue;
                    if (curUser.Token != FSQOAuth.Token)
                    {
                        curUser.Token = FSQOAuth.Token;
                    }
                    UpdateModel(curUser);
                    fsqDBContext.SaveChanges();
                    FormsAuthentication.SetAuthCookie(mUser.UserName, true);
                }
                else
                {
                    curUser = new FoursquareUserModel();
                    curUser.FoursquareUserId = userId;
                    curUser.Token = FSQOAuth.Token;
                    string password = Guid.NewGuid().ToString();

                    MembershipUser mUser;
                    try
                    {
                        mUser = Membership.CreateUser(userId.ToString(), password);
                    }
                    catch (Exception)
                    {
                        mUser = Membership.FindUsersByName(userId.ToString())[userId.ToString()];
                    }
                    curUser.UserGuid = (Guid)mUser.ProviderUserKey;
                    curUser.UserName = userId.ToString();
                    curUser.LastVenueID = lastVenue;
                    fsqDBContext.FoursquareUsers.Add(curUser);
                    fsqDBContext.SaveChanges();
                    FormsAuthentication.SetAuthCookie(curUser.UserName, true);
                }
            }
           
            return RedirectToAction("Index");


        }

    }

    
}
