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
            
            if (AuthService.ValidateAuthData(uId, token))
            {
                FoursquareUserContext dbContext = new FoursquareUserContext();
                IMessageRepository repository = new MessageRepository(dbContext);
                FoursquareOAuth fsqOAuth = new FoursquareOAuth(token);
                if (fsqOAuth.CheckForFriendship(tId))
                {
                    Profile res = fsqOAuth.GetProfileInfo(tId);
                    return res; 
                }
                
            }
            return null;
        }

        public Profile GetMyProfile(string userId, string token)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                FoursquareOAuth fsqOAuth = new FoursquareOAuth(token);
                Profile res = fsqOAuth.GetProfileInfo(uId);
                return res;
            }
            return null;
        }

        public bool GetMakeFriends(string token, string userId, string targetId)
        {
            int uId = Convert.ToInt32(userId);
            int tId = Convert.ToInt32(targetId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                FoursquareOAuth foursquareOAuth = new FoursquareOAuth(token);
                
                MessageModel messageModel = new MessageModel()
                {
                    From = uId,
                    Message = "Accept",
                    time = DateTime.Now,
                    To = tId,
                    type = "Invite"
                };
                IMessageRepository repository = new MessageRepository(new FoursquareUserContext());
                repository.InsertMessage(messageModel);
                repository.Save();
                return foursquareOAuth.MakeFriendship(tId);
            }
            return false;
        }

        public bool GetApproveFriend(string token, string userId, string targetId)
        {
            int uId = Convert.ToInt32(userId);
            int tId = Convert.ToInt32(targetId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                FoursquareOAuth foursquareOAuth = new FoursquareOAuth(token);
                return foursquareOAuth.ApproveFriendship(tId);
            }
            return false;
        }



    }
}
