using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _4sqChat.Models;

namespace _4sqChat.Logic
{
    public class AuthService
    {
        public static bool ValidateAuthData(int userId, string token)
        {
            FoursquareUserContext dbContent = new FoursquareUserContext();
            FoursquareUserModel user = dbContent.FoursquareUsers.Find(userId);

            if (user == null)
                return false;

            if (user.FoursquareUserId == userId && token == user.Token)
                return true;
            return false;
        }
    }
}