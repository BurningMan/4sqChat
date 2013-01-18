using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace _4sqApp.Logic
{
    class MessageModel
    {
        public Guid messageID;
        public int From;
        public int To;
        public string Message;
        public DateTime time;
    }
}
