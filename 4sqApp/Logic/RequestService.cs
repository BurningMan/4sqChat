using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _4sqApp.Logic
{
    class RequestService
    {
        

        public static void MakeGetRequest(AuthData authData, string url, Dictionary<string, string> param, DownloadStringCompletedEventHandler handler)
        {
            url += "?";
            foreach (KeyValuePair<string, string> keyValuePair in param)
            {
                url += String.Format("{0}={1}&", keyValuePair.Key, keyValuePair.Value);
            }

            if (url[url.Length - 1] == '&')
            {
                url = url.Substring(0, url.Length - 1);
            }

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += handler;
            wc.DownloadStringAsync(new Uri(url));
            
        }

        
    }
}
