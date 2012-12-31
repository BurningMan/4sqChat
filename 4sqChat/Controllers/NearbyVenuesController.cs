using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using _4sqChat.Logic;
using _4sqChat.Models;


namespace _4sqChat.Controllers
{
    public class NearbyVenuesController : ApiController
    {
        private FoursquareOAuth foursquareOAuth;
        public List<FoursquareOAuth.Venue> nv = new List<FoursquareOAuth.Venue>();
        private List<FoursquareOAuth.Venue> NearbyVenues;
        private void InitializeOauth()
        {
            foursquareOAuth = new FoursquareOAuth(CurrentUserToken());
        }
        public void FillNV()
        {
            InitializeOauth();
            NearbyVenues = new List<FoursquareOAuth.Venue>();
            List<String> NearbyVenuesIds = foursquareOAuth.GetNearbyVenues();
            foreach (var nearbyVenuesId in NearbyVenuesIds)
            {
                NearbyVenues.Add(foursquareOAuth.GetVenuesInfo(nearbyVenuesId));
            }
        }
        public List<FoursquareOAuth.Venue> GetAllVenues()
        {
            FillNV();
            return NearbyVenues;
        }

        public FoursquareOAuth.Venue GetVenueInfo(string id)
        {
            InitializeOauth();
            return foursquareOAuth.GetVenuesInfo(id);
        }

        public string CurrentUserToken()
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
