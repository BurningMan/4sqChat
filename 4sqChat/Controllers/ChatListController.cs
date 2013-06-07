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
        public IEnumerable<KeyValuePair<int, DateTime>> GetChats(string userId, string token)
        {
            int uId = Convert.ToInt32(userId);
            if (AuthService.ValidateAuthData(uId, token))
            {
                IEnumerable<MessageModel> messages =
                    repository.GetMessages().Where(model => model.From == uId || model.To == uId).
                    OrderByDescending(model => model.time);
                List<int> res = new List<int>();
                foreach (MessageModel messageModel in messages)
                {
                    if (messageModel.From != uId)
                        res.Add(messageModel.From);
                    else
                        res.Add(messageModel.To);
                }
                res = res.Distinct().ToList();
                List<KeyValuePair<int, DateTime>> final = new List<KeyValuePair<int, DateTime>>();
                foreach (var usId in res)
                {
                    DateTime dt = messages.Where(model => model.From == usId || model.To == usId).
                                           OrderByDescending(model => model.time).Select(model => model.time).First();
                    final.Add(new KeyValuePair<int, DateTime>(usId, dt));
                    
                }
                return final;
            }

            return null;
        }

    }
}
