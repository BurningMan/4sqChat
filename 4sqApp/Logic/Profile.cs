using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
namespace _4sqApp.Logic
{

    public class Profile
    {
        [JsonProperty("Photo")]
        public string Photo;
        [JsonProperty("FirstName")]
        public string FirstName;
        [JsonProperty("lastName")]
        public string lastName;
        [JsonProperty("homecity")]
        public string homecity;
        [JsonProperty("gender")]
        public string gender;
        [JsonProperty("scoreMAX")]
        public string scoreMAX;

        public Profile(string photo, string firstName, string lastName, string homecity, string gender, string scoreMax)
        {
            Photo = photo;
            FirstName = firstName;
            this.lastName = lastName;
            this.homecity = homecity;
            this.gender = gender;
            scoreMAX = scoreMax;
        }
    }
}
