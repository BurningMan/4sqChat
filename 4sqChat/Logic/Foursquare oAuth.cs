using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Mvc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace _4sqChat.Logic
{
    public class Foursquare_oAuth{
        //https://github.com/aravamudham/foursquare-oauth-c--library/blob/master/FSquare.aspx.cs
        public oAuth4Square oAuth;
        private string token { get; set; }
        public Foursquare_oAuth(string consumerKey, string consumerSecret, string callbackUrl)
        {
            oAuth = new oAuth4Square();
            oAuth.ConsumerKey = consumerKey;
            oAuth.ConsumerSecret = consumerSecret;
            oAuth.CallBackUrl = callbackUrl;
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
            }
            catch (Exception oe)
            {
                return false;
            }
            return true;
        }

        private FSquareToken GetFSquareTokenDetails(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FSquareToken));
            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                FSquareToken list = (FSquareToken)serializer.ReadObject(stream);
                return list;
            }
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