using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using _4sqApp.Logic;

namespace _4sqApp
{
    public partial class VenueInfo : PhoneApplicationPage
    {
        public static Venue venue;
        public VenueInfo()
        {
            InitializeComponent();
            VenueInfoTb.Text = "Name " + venue.Name + "\r\n Contact " + venue.Contact + "\r\n Address " + venue.address +
                              "\r\n Category " + venue.category;
        }

    }
}