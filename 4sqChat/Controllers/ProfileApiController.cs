using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using _4sqChat.Logic;
using _4sqChat.Models;
using System.Configuration;

namespace _4sqChat.Controllers
{
    public class ProfileApiController : ApiController
    {
        public Profile GetProfile(string userId, string token, string targetId)
        {
            int uId = Convert.ToInt32(userId);
            int tId = Convert.ToInt32(targetId);
            int messageCount = Convert.ToInt32(ConfigurationManager.AppSettings["ProfileShowMessageCount"]);
            if (AuthService.ValidateAuthData(uId, token))
            {
                FoursquareUserContext dbContext = new FoursquareUserContext();
                IMessageRepository repository = new MessageRepository(dbContext);
                FoursquareOAuth fsqOAuth = new FoursquareOAuth(token);
                int c1 = repository.GetMessagesByKey(uId, tId).Count();
                int c2 = repository.GetMessagesByKey(tId, uId).Count();
                if (c1 + c2 > messageCount)
                {
                    Profile res = fsqOAuth.GetProfileInfo(tId);
                    return res;
                }
            }
            return null;
        }

    }
}
