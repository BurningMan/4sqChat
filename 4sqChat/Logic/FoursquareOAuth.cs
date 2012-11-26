using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Configuration;
using System.Web.Mvc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using _4sqChat.Models;

namespace _4sqChat.Logic
{
    public class FoursquareOAuth
    {
        //https://github.com/aravamudham/foursquare-oauth-c--library/blob/master/FSquare.aspx.cs
        public oAuth4Square oAuth;
        private string token;

        public string Token
        {
            get { return token; }
            set { this.token = value; }
        }

        public FoursquareOAuth(string userToken)
        {
            oAuth = new oAuth4Square();
            oAuth.ConsumerKey = ConfigurationManager.AppSettings["FSQClientID"];
            oAuth.ConsumerSecret = ConfigurationManager.AppSettings["FSQClientSecret"];
            oAuth.CallBackUrl = ConfigurationManager.AppSettings["FSQCallback"];
            Token = userToken;
        }

        public string GetAuthURL()
        {
            string res = "{0}?client_id={1}&response_type=code&redirect_uri={2}";
            res = String.Format(res, oAuth.oAuthRequestToken, oAuth.ConsumerKey, oAuth.CallBackUrl);
            return res;
        }

        public bool makeAuthentication(string code)
        {
            string retjson;
            FSquareToken fstoken = new FSquareToken();
            try
            {
                retjson = oAuth.oAuthRequest(code);
                fstoken = GetFSquareTokenDetails(retjson);
                //the authenticated token we get back from foursquare
                token = fstoken.AccessToken;
                oAuth.Token = token;
            }
            catch (Exception oe)
            {
                return false;
            }
            return true;
        }

        private FSquareToken GetFSquareTokenDetails(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof (FSquareToken));
            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                FSquareToken list = (FSquareToken) serializer.ReadObject(stream);
                return list;
            }
        }
        // This method has no logical usage in this version of application , but can be possibly working in later versions
        // Working
        /*
        public bool CheckIn(string venueID)
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "checkins/add?oauth_token="+token;
            
            NameValueCollection postData = new NameValueCollection();
            postData["venueId"] = venueID;
            HttpPost(reqURL, postData);
            return true;
        }
        
         */
        public string GetLastVenue()
        {
           string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/self/venuehistory";
           NameValueCollection nv = new NameValueCollection();
           nv["oauth_token"] = token;
            nv["afterTimestamp"] = Convert.ToString(Convert.ToInt64((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds - 15000));
           string result = HttpGet(reqURL, nv);
           JObject obj = JObject.Parse(result);
            if ((int) obj["response"]["venues"]["count"] == 0)
                return null;
           return (string)obj["response"]["venues"]["items"][0]["venue"]["id"];
        }

        private NameValueCollection GetLL()
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "venues/" + GetLastVenue();
            NameValueCollection nv = new NameValueCollection();
            nv["oauth_token"] = token;
            //nv["venueID"] = GetLastVenue();
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            var LL = new NameValueCollection();
            string lat = Convert.ToString(obj["response"]["venue"]["location"]["lat"]);
            lat = lat.Replace(',', '.');
            string lng = Convert.ToString(obj["response"]["venue"]["location"]["lng"]);
            lng = lng.Replace(',', '.');
            LL["ll"] = String.Format("{0},{1}", lat, lng);
            return LL;
            
            
        }
        public NameValueCollection GetVenuesInfo(string venueID)
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "venues/"+venueID;
            var nv = new NameValueCollection();
            nv["oauth_token"] = token;
            string result = HttpGet(reqURL,nv);
            nv.Clear();
            JObject obj = JObject.Parse(result);
            nv["name"] =  "" +(string)obj["response"]["venue"]["name"];
            nv["contact"] = "" + (string)obj["response"]["venue"]["contact"]["phone"];
            nv["address"] = "" + (string)obj["response"]["venue"]["location"]["address"];
            nv["cat"] = "" + (string)obj["response"]["venue"]["categories"][0]["name"];
            return nv;
        }
        public List<string> GetNearbyVenues()
        {
            if (GetLastVenue() == null)
            {
                return null;
            }
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "venues/search";
            NameValueCollection nv = GetLL();
            nv["oauth_token"] = token;
            nv["radius"] = "1000";
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            var venues = new List<string>();
            foreach (var venue in obj["response"]["groups"][0]["items"])
            {
                venues.Add((string) venue["id"]);
            }
            return venues;
        }

        public List<int> GetNearByUsers()
        {
            Models.FoursquareUserContext db = new FoursquareUserContext();
            List<string> venueList = GetNearbyVenues();
            List<int> res = new List<int>();
            int userId = GetUserId();
            foreach (string s in venueList)
            {
                IEnumerable<int> q = from o in db.FoursquareUsers where o.LastVenueID == s && o.FoursquareUserId != userId  select o.FoursquareUserId;
                res.AddRange(q);
            }
            return res;
        }

        public int GetUserId()
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/self";
            NameValueCollection nv = new NameValueCollection();
            nv["oauth_token"] = token;
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            return Convert.ToInt32((string)obj["response"]["user"]["id"]);
        }
        

        private static string HttpPost(string uri, NameValueCollection data)
        {
            byte[] response = null;
            using (var wb = new WebClient())
            {
                
                response = wb.UploadValues(uri, "POST", data);
            }
            return Encoding.UTF8.GetString(response,0, response.Length);
        }

        private static string HttpGet(string uri, NameValueCollection data)
        {
            string request = "?";
            foreach (string key in data.AllKeys)
            {
                request += String.Format("{0}={1}", key, data[key])+"&";
            }
            request = request.Substring(0, request.Length - 1);
            request = uri + request;
            string res;
            using (var wb = new WebClient())
            {
                res = wb.DownloadString(request);
            }
            return res;
        }
        public Profile GetProfileInfo(int Target_ID)
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/"+Target_ID;
            var nv = new NameValueCollection();
            nv["oauth_token"] = token;
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            nv.Clear();
            
            nv["FirstName"] =""+ Convert.ToString(obj["response"]["user"]["firstName"]);
            nv["LastName"] = "" + Convert.ToString(obj["response"]["user"]["lastName"]);
            nv["Photo"] = "" + Convert.ToString(obj["response"]["user"]["photo"]);
            nv["Gender"] = "" + Convert.ToString(obj["response"]["user"]["gender"]);
            nv["Homecity"] = "" + Convert.ToString(obj["response"]["user"]["homeCity"]);
            nv["scoremax"] = "" + Convert.ToString(obj["response"]["user"]["scores"]["max"]);
            Profile targetProfile = new Profile(nv);
            return targetProfile;
        }
    }

    [DataContract()]
    public class FSquareToken
    {
        private string f_accesstoken;

        [DataMember(Name = "access_token")]
        public string AccessToken
        {
            get { return this.f_accesstoken; }

            set { this.f_accesstoken = value; }
        }
    }
}