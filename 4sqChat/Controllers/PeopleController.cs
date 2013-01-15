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
        private string CurrentUserToken()
        {
            if (!User.Identity.IsAuthenticated)
                return null;
            Models.FoursquareUserContext fsqDBContext = new FoursquareUserContext();
            Models.FoursquareUserModel um = fsqDBContext.FoursquareUsers.Find(Convert.ToInt32(User.Identity.Name));
            if (um != null)
                return um.Token;
            return null;
        }
        private void InitializeOauth()
        {
            foursquareOAuth = new FoursquareOAuth(CurrentUserToken());
        }

        public List<Profile> GetNearbyUsers()
        {
            InitializeOauth();
            List<int> nearbyUsersIds = foursquareOAuth.GetNearByUsers();
            List<Profile> nearbyUsers = new List<Profile>();
            foreach (var nearbyUsersId in nearbyUsersIds)
            {
                nearbyUsers.Add(foursquareOAuth.GetProfileInfo(nearbyUsersId));
            }
            return nearbyUsers;
        }

        public Profile GetNearbyUserById(int id)
        {
            InitializeOauth();
            return foursquareOAuth.GetProfileInfo(id);
        }
    }
}
