using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace _4sqChat.Models
{
    public interface IMessageRepository : IDisposable
    {
        IEnumerable<MessageModel> GetMessages();
        IEnumerable<MessageModel> GetMessagesByKey(int from, int to);
        void InsertMessage(MessageModel message);
        void Save();
    }

    public class MessageRepository : IMessageRepository, IDisposable
    {
        private FoursquareUserContext context;

        public MessageRepository(FoursquareUserContext context)
        {
            this.context = context;
        }

        public IEnumerable<MessageModel> GetMessages()
        {
            return context.Messages.ToList();
        }

        public IEnumerable<MessageModel> GetMessagesByKey(int from, int to)
        {
            return context.Messages.Where(m => (m.To == to && m.From == from));
        }

        public void InsertMessage(MessageModel message)
        {
            context.Messages.Add(message);
        }

        public void Save()
        {
            context.SaveChanges();
        }
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    

    public class MessageModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid messageID { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string Message { get; set; }
        public DateTime time { get; set; }
        public String type { get; set; }
    }
}