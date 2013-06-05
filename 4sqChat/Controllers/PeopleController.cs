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

        public List<Profile> GetNearbyUsers(string userId, string token)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                InitializeOauth(token);
                try
                {
                    //TODO add param
                    List<int> nearbyUsersIds = foursquareOAuth.GetNearByUsers(1000);
                    List<Profile> nearbyUsers = new List<Profile>();
                    foreach (var nearbyUsersId in nearbyUsersIds)
                    {
                        nearbyUsers.Add(foursquareOAuth.GetProfileInfo(nearbyUsersId));
                    }
                    return nearbyUsers;
                }
                catch (Exception e)
                {
                    return new List<Profile>();
                }
            }
            return null;
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


        public List<int> GetNearByIds(string userId, string token, string a, string b)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                InitializeOauth(token);
                try
                {

                    //TODO add param
                    List<int> nearbyUsersIds = foursquareOAuth.GetNearByUsers(1000);
                    return nearbyUsersIds;
                }
                catch (Exception e)
                {
                    return new List<int>();
                }
            }
            return null;
        }
    }
}
