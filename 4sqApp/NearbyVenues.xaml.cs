using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using _4sqApp.Logic;

namespace _4sqApp
{
    public partial class NearbyVenues : PhoneApplicationPage
    {
        public static AuthData authData;
        private List<Venue> venues;
        private bool nodata = false;
        public NearbyVenues()
        {

            InitializeComponent();
            GetVenues();
        }

        public void DownloadStringHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            string res = e.Result;
            if (res != null)
            {
                venues = JsonConvert.DeserializeObject<List<Venue>>(res);
                if (venues.Count == 0)
                {
                    VenuesPanel.Children.Add(
                        createNotepadeButton("There are no venues nearby or you have no recent checkins", "-1"));
                    nodata = true;
                }
                else
                {
                    nodata = false;
                    foreach (Venue venue in venues)
                    {
                        VenuesPanel.Children.Add(createNotepadeButton(venue.Name, venue.id));

                    }
                }
            }
        }

        public void GetVenues()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["userId"] = authData.userId.ToString();
            parameters["token"] = authData.token;
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            RequestService.MakeGetRequest(authData, (string) settings["appURL"] + "api/NearbyVenues", parameters,
                                          DownloadStringHandler);
        }

        private Button createNotepadeButton(String title,string id)
        {
            Button button = new Button();
            button.Width = 438;
            button.Height = 100;
            button.Content = title.ToUpper();
            button.FontSize = 20;
            button.Click += new RoutedEventHandler(buttonClick);
            button.FontSize = 16;
            button.Name = id;
            if (nodata)
                button.IsEnabled = false;
            return button;
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            Button item = (Button) sender;
            foreach (Venue venue in venues)
            {
                if (venue.id == item.Name)
                {
                    VenueInfo.venue = venue;
                    break;
                }
            }
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/VenueInfo.xaml", UriKind.Relative)));
            
        }


    }
}