using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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

    }
}
