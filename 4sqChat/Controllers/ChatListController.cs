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
    public class ChatListController : ApiController
    {
        private IMessageRepository repository = new MessageRepository(new FoursquareUserContext());
        public IEnumerable<int> GetChats(string userId, string token)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                IEnumerable<MessageModel> messages =
                    repository.GetMessages().Where(model => model.From == uId || model.To == uId);
                List<int> res = new List<int>();
                foreach (MessageModel messageModel in messages)
                {
                    if (messageModel.From != uId)
                        res.Add(messageModel.From);
                    else
                        res.Add(messageModel.To);
                }
                return res.Distinct();
            }

            return null;
        }
    }
}
