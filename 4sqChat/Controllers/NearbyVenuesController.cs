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
        private void InitializeOauth(string token)
        {
            foursquareOAuth = new FoursquareOAuth(token);
        }

        public void FillNV(string token)
        {
            InitializeOauth(token);
            NearbyVenues = new List<FoursquareOAuth.Venue>();
            try
            {
                //TODO add parameters
                List<String> NearbyVenuesIds = foursquareOAuth.GetNearbyVenues(1000);
                foreach (var nearbyVenuesId in NearbyVenuesIds)
                {
                    NearbyVenues.Add(foursquareOAuth.GetVenuesInfo(nearbyVenuesId));
                }
            }
            catch (Exception e)
            {
                NearbyVenues = new List<FoursquareOAuth.Venue>();
            }
        }

        public List<FoursquareOAuth.Venue> GetAllVenues(string userId, string token)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                FillNV(token);
                return NearbyVenues;
            }
            return null;
        }

        public FoursquareOAuth.Venue GetVenueInfo(string userId, string token, string id)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token)) 
            {
                InitializeOauth(token);
                return foursquareOAuth.GetVenuesInfo(id);
            }
            return new FoursquareOAuth.Venue();
        }

    }

}
