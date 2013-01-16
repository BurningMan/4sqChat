using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using _4sqChat.Logic;
using _4sqChat.Models;

namespace _4sqChat.Controllers
{
    public class PeopleController : ApiController
    {
        private FoursquareOAuth foursquareOAuth;
      
        private void InitializeOauth(string token)
        {
            foursquareOAuth = new FoursquareOAuth(token);
        }

        public List<Profile> GetNearbyUsers(string token)
        {
            InitializeOauth(token);
            List<int> nearbyUsersIds = foursquareOAuth.GetNearByUsers();
            List<Profile> nearbyUsers = new List<Profile>();
            foreach (var nearbyUsersId in nearbyUsersIds)
            {
                nearbyUsers.Add(foursquareOAuth.GetProfileInfo(nearbyUsersId));
            }
            return nearbyUsers;
        }

        public Profile GetNearbyUserById(string userId, string token, int id)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                InitializeOauth(token);
                return foursquareOAuth.GetProfileInfo(id);
            }
            return null;
        }
    }
}
