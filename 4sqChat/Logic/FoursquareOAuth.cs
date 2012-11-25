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

        public bool CheckIn(string venueID)
        {
            string reqURL = ConfigurationManager.AppSettings["FSQApi"] + "checkins/add?oauth_token="+token;
            
            NameValueCollection postData = new NameValueCollection();
            postData["venueId"] = venueID;
            HttpPost(reqURL, postData);
            return true;
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