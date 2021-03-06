﻿using System;
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
using log4net;

namespace _4sqChat.Logic
{
    public class FoursquareOAuth
    {
        public struct Venue
        {
            public string id { get; set; }
            public string Name { get; set; }
            public string Contact { get; set; }
            public string address { get; set; }
            public string category { get; set; }
        }

        private ILog logger = LogManager.GetLogger(typeof (FoursquareOAuth));

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

        /// <summary>
        /// Authenticates user using code from Foursquare response
        /// </summary>
        /// <param name="code">Response code</param>
        /// <returns></returns>
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
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/self/checkins";
            NameValueCollection nv = new NameValueCollection();
            nv["oauth_token"] = token;
            nv["afterTimestamp"] =
                Convert.ToString(
                    Convert.ToInt64((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds - 55000));
            nv["sort"] = "newestfirst";
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            if ((int) obj["response"]["checkins"]["items"].Count() == 0)
                return null;
            return (string) obj["response"]["checkins"]["items"][0]["venue"]["id"];
        }

        /// <summary>
        /// Gets venue latitude and lontitude
        /// </summary>
        /// <returns>Latitude and lontitude separated by commas</returns>
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


        public Venue GetVenuesInfo(string venueID)
        {
            logger.Debug("Getting venue info for " + venueID);
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "venues/" + venueID;
            var nv = new NameValueCollection();
            nv["oauth_token"] = token;
            string result = HttpGet(reqURL, nv);
            logger.Debug(result);
            nv.Clear();
            JObject obj = JObject.Parse(result);
            Venue venue = new Venue();
            venue.id = "" + (string) obj["response"]["venue"]["id"];
            venue.Name = "" + (string) obj["response"]["venue"]["name"];
            venue.Contact = "" + (string) obj["response"]["venue"]["contact"]["phone"];
            venue.address = "" + (string) obj["response"]["venue"]["location"]["address"];
            venue.category = "" + (string) obj["response"]["venue"]["categories"][0]["name"];
            /*nv["name"] = "" + (string)obj["response"]["venue"]["name"];
            nv["contact"] = "" + (string)obj["response"]["venue"]["contact"]["phone"];
            nv["address"] = "" + (string)obj["response"]["venue"]["location"]["address"];
            nv["cat"] = "" + (string)obj["response"]["venue"]["categories"][0]["name"];*/
            return venue;
        }


        public List<string> GetNearbyVenues(long radius)
        {
            if (GetLastVenue() == null)
            {
                return null;
            }
            logger.Debug("Got last venue");
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "venues/search";
            NameValueCollection nv = GetLL();
            nv["oauth_token"] = token;
            nv["radius"] = radius.ToString();
            nv["intent"] = "browse";
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            var venues = new List<string>();
            foreach (var venue in obj["response"]["groups"][0]["items"])
            {
                venues.Add((string) venue["id"]);
            }
            return venues;
        }


        
        public List<int> GetNearByUsers(int radius)
        {
            Models.FoursquareUserContext db = new FoursquareUserContext();
            
            List<string> venueList = GetNearbyVenues(radius);
            if (venueList == null)
            {
                return new List<int>();
            }
            List<int> res = new List<int>();
            int userId = GetUserId();
            foreach (string s in venueList)
            {
                IEnumerable<int> q = from o in db.FoursquareUsers
                                     where o.LastVenueID == s && o.FoursquareUserId != userId
                                     select o.FoursquareUserId;
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
            return Convert.ToInt32((string) obj["response"]["user"]["id"]);
        }

        /// <summary>
        /// Makes POST request
        /// </summary>
        /// <param name="uri">Request URI</param>
        /// <param name="data">POST parameters</param>
        /// <returns>Request result</returns>
        private static string HttpPost(string uri, NameValueCollection data)
        {
            byte[] response = null;
            using (var wb = new WebClient())
            {
                response = wb.UploadValues(uri, "POST", data);
            }
            Encoding cp1251 = Encoding.GetEncoding("windows-1251");
            return cp1251.GetString(response, 0, response.Length);
        }


        public static string Utf8toUtf16(byte[] source)
        {
            Encoding utf8 = Encoding.UTF8;
            string text = utf8.GetString(source);
            return text;
        }

        private static string HttpGet(string uri, NameValueCollection data)
        {
            string request = "?";
            foreach (string key in data.AllKeys)
            {
                request += String.Format("{0}={1}", key, data[key]) + "&";
            }
            request = request.Substring(0, request.Length - 1);
            request = uri + request;
            string res;
            byte[] bytes;
            using (var wb = new WebClient())
            {
                bytes = wb.DownloadData(request);
            }
            return Utf8toUtf16(bytes);
        }

        /// <summary>
        /// Profile info for user
        /// </summary>
        /// <param name="Target_ID">Foursquare User ID</param>
        /// <returns>User Profile</returns>
        public Profile GetProfileInfo(int Target_ID)
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/" + Target_ID;
            var nv = new NameValueCollection();
            nv["oauth_token"] = token;
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            nv.Clear();

            nv["FirstName"] = "" + Convert.ToString(obj["response"]["user"]["firstName"]);
            nv["LastName"] = "" + Convert.ToString(obj["response"]["user"]["lastName"]);
            nv["Photo"] = "" + Convert.ToString(obj["response"]["user"]["photo"]);
            nv["Gender"] = "" + Convert.ToString(obj["response"]["user"]["gender"]);
            nv["Homecity"] = "" + Convert.ToString(obj["response"]["user"]["homeCity"]);
            nv["scoremax"] = "" + Convert.ToString(obj["response"]["user"]["scores"]["max"]);
            Profile targetProfile = new Profile(nv);
            return targetProfile;
        }

        public bool MakeFriendship(int targetId)
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/" + targetId +
                            "/request?oauth_token=" + token;

            var nv = new NameValueCollection();
            string result = null;
            try
            {
                result = HttpPost(reqURL, nv);
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                    return true;
                return false;
            }
            JObject obj = JObject.Parse(result);
            nv.Clear();

            return obj["response"]["user"] != null;
        }

        public bool CheckForFriendship(int targetId)
        {
            List<int> friends = GetFriends();
            for (int i = 0; i < friends.Count; ++i)
            {
                if (friends[i] == targetId)
                    return true;
            }

            return false;
        }

        public bool ApproveFriendship(int targetId)
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/" + targetId + "/approve?oauth_token=" +
                            token;
            var nv = new NameValueCollection();
            string result = HttpPost(reqURL, nv);
            JObject obj = JObject.Parse(result);
            return obj["response"]["user"] != null;
        }

        public List<int> GetFriends()
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "users/self/friends";
            var nv = new NameValueCollection();
            nv["oauth_token"] = token;
            string result = HttpGet(reqURL, nv);
            JObject obj = JObject.Parse(result);
            nv.Clear();

            int count = Convert.ToInt32(obj["response"]["friends"]["count"]);
            List<int> users = new List<int>();
            for (int i = 0; i < count; ++i)
            {
                int userId = Convert.ToInt32(obj["response"]["friends"]["items"][i]["id"]);
                users.Add(userId);
            }
            return users;
        }

        public List<string> NearbyVenuesInRange(int minRadius, int maxRadius)
        {
            HashSet<string> venuesSet = new HashSet<string>();
            List<string> minRadiusVenues = minRadius <= 0 ? new List<string>() : GetNearbyVenues(minRadius);

            List<string> res = GetNearbyVenues(maxRadius);

            if (res == null)
            {
                return null;
            }

            for (int i = 0; i < res.Count; ++i)
            {
                venuesSet.Add(res[i]);
            }
            venuesSet.ExceptWith(minRadiusVenues);
            res = venuesSet.ToList();
            return res;
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