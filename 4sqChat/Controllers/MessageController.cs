using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using _4sqChat.Models;

namespace _4sqChat.Controllers
{
    public class MessageController : ApiController
    {
        private IMessageRepository repository = new MessageRepository(new FoursquareUserContext());

        public IEnumerable<MessageModel> GetAllMessages()
        {
            return repository.GetMessages();
        }

        public IEnumerable<MessageModel> GetMessagesByKeys(string addr)
        {
            string[] tmp = addr.Split('_');
            int from = Convert.ToInt32(tmp[0]);
            int to = Convert.ToInt32(tmp[1]);
            return repository.GetMessagesByKey(from, to).TakeWhile(m => (m.time - DateTime.Now).TotalHours < 2);
        }

        
        public bool SendMessage(int from, int to, string message)
        {
            MessageModel m = new MessageModel();
            m.To = to;
            m.From = from;
            m.Message = message;
            m.time = DateTime.Now;
            repository.InsertMessage(m);
            repository.Save();
            return true;
        }
    }
}
