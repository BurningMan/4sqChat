using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using _4sqApp.Logic;

namespace _4sqApp
{
    public partial class UserProfile : PhoneApplicationPage
    {
        public static long profileId;
        public static AuthData authData;
        private string result;
        public UserProfile()
        {
            InitializeComponent();
            GetProfile(Convert.ToInt32(profileId));
        }

        public void DownloadStringHandler(object sender, DownloadStringCompletedEventArgs eventArgs)
        {
            result = eventArgs.Result;
            Profile profile = JsonConvert.DeserializeObject<Profile>(result);
            Image1.Source = new BitmapImage(new Uri(profile.Photo));
            textblock1.Text = "Name " + profile.FirstName + "\r\n" +
                              profile.lastName + "\r\n" + "Gender " + profile.gender + "\r\n" + "Homecity " +
                              profile.homecity + "\r\n" + "MaxScore " + profile.scoreMAX;
        }

        public void GetProfile(int userId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["userId"] = authData.userId.ToString();
            parameters["token"] = authData.token;
            parameters["targetId"] = userId.ToString();
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            RequestService.MakeGetRequest(authData, (string)settings["appURL"] + "api/ProfileApi", parameters,
                                          DownloadStringHandler);
        }
    }
}