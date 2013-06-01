using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using _4sqApp.Logic;

namespace _4sqApp
{
    public partial class ChatList : PhoneApplicationPage
    {
        public static AuthData authData;
        private List<int> chats; 
        public ChatList()
        {
            
            InitializeComponent();
            GetChats();
        }
        public void DownloadStringHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            string res = e.Result;
            if (res != null)
            {
                 chats = JsonConvert.DeserializeObject<List<int>>(res);
                foreach (int chat in chats)
                {

                    stackpanel1.Children.Add(createNotepadeButton(chat.ToString()));

                }
            }
        }
        public void GetChats()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["userId"] = authData.userId.ToString();
            parameters["token"] = authData.token;
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            RequestService.MakeGetRequest(authData, (string)settings["appURL"] + "api/ChatList", parameters,
                                          DownloadStringHandler);
        }

        private Button createNotepadeButton(String id)
        {
            Button button = new Button();
            button.Width = 300;
            button.Height = 100;
            button.FontSize = 20;
            button.Click += new RoutedEventHandler(buttonClick);
            button.FontSize = 16;
            button.Name = id;
            button.Content = id;
            return button;
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            Button item = (Button)sender;
            int targetId = 0;
            foreach (int chat in chats)
            {
                if (chat.ToString() == item.Name)
                {
                    targetId = chat;
                    //link to chat
                    break;
                }
            }
            Chat.authData = authData;
            Chat.To = targetId;
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Chat.xaml", UriKind.Relative)));

        }
    }
}