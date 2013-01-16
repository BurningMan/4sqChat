using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace _4sqChat.Models
{
    [DataContract]
    public class Profile
    {
        [DataMember]
        private string Photo;
        [DataMember]
        private string FirstName;
        [DataMember]
        private string lastName;
        [DataMember]
        private string homecity;
        [DataMember]
        private string gender;
        [DataMember]
        private string scoreMAX;

        private NameValueCollection getInfoStandart()
        {
            var nv = new NameValueCollection();
            nv["firstname"] = FirstName;
            nv["homecity"] = homecity;
            nv["gender"] = gender;
            nv["GetPremium"] = "Want More info? Get Premium right now!";
            return nv;
        }
        
        private NameValueCollection getInfoPremium()
        {
            var nv = new NameValueCollection();
            nv["firstname"] = FirstName;
            nv["lastname"] = lastName;
            nv["homecity"] = homecity;
            nv["gender"] = gender;
            nv["scoremax"] = scoreMAX;
            nv["Photo"] = Photo;
            return nv;
        }

        public NameValueCollection getInfo(bool isPremium)
        {
            return (isPremium ? getInfoPremium() : getInfoStandart());
        }

        public Profile(NameValueCollection nv)
        {
            this.FirstName =nv["FirstName"];
            this.Photo = nv["Photo"];
            this.lastName = nv["lastName"];
            this.homecity = nv["Homecity"];
            this.gender = nv["Gender"];
            this.scoreMAX = nv["scoremax"];
        }
    }
}