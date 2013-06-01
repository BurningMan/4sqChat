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

        private static int CompareMessagesByDate(MessageModel a, MessageModel b)
        {
            if (a.time > b.time)
                return 1;
            if (b.time > a.time)
                return -1;
            return 0;
        }

        public IEnumerable<MessageModel> GetMessagesByKeys(string keys)
        {
            string[] tmp = keys.Split('_');
            int from = Convert.ToInt32(tmp[0]);
            int to = Convert.ToInt32(tmp[1]);
            List<MessageModel> r = new List<MessageModel>();
            
            IEnumerable<MessageModel> res =
                repository.GetMessagesByKey(from, to).OrderByDescending(m => m.time).Take(10);
            r.AddRange(res);
            res = repository.GetMessagesByKey(to, from).OrderByDescending(m => m.time).Take(10);
            r.AddRange(res);
            r.Sort(CompareMessagesByDate);
            return r;
        }

        
        public bool GetSendMessage(string from, string to,string messag)
        {
            
            string message = messag;
            MessageModel m = new MessageModel();
            m.To = Convert.ToInt32(to);
            m.From = Convert.ToInt32(from);
            m.Message = message;
            m.time = DateTime.Now;
            m.type = "Message";
            repository.InsertMessage(m);
            repository.Save();

            return true;
        }

        public int GetMessageCount(string from, string to)
        {
            int f = Convert.ToInt32(from);
            int t = Convert.ToInt32(to);
            int count = repository.GetMessagesByKey(f, t).Count();
            count += repository.GetMessagesByKey(t, f).Count();
            return count;
        }

    }
}
