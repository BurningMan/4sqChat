using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using _4sqApp.Logic;

namespace _4sqApp
{
    public partial class Chat : PhoneApplicationPage
    {
        private Profile target;
        private Profile user;
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        public static AuthData authData;
        public static long To = -1;
        private Timer timer;
        private bool profileEnabled = false;
        public Chat()
        {
            InitializeComponent();
            if(To==-1)
            GetUserToChat();
            timer = new Timer(GetMessages, null, 0, 2000);
            
        }

        public void GetUserToChat()
        {
            string url = settings["appURL"] + "api/People";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["userId"] = authData.userId.ToString();
            parameters["token"] = authData.token;
            parameters["a"] = "tmp";
            parameters["b"] = "tmp";
            RequestService.MakeGetRequest(authData, url, parameters, DownloadPeopleHandler);
        }

        public void GetMessageCount()
        {
            string url = settings["appURL"] + "api/message";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["from"] = authData.userId.ToString();
            parameters["to"] = To.ToString();
            RequestService.MakeGetRequest(authData, url, parameters, DownloadMessageCountHandler);
        }

        public void DownloadMessageCountHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            string res = e.Result;
            int count = JsonConvert.DeserializeObject<int>(res);
            if (count > 50)
            {
                Dispatcher.BeginInvoke(() => EnableProfileButton());
            }
        }

        public void EnableProfileButton()
        {
            if (!profileEnabled)
            {
                ProfileButton.IsEnabled = true;
                profileEnabled = true;
                GetProfiles();
            }
        }

        public void DownloadPeopleHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            string res = e.Result;
            if (res == null)
                To = -1;
            else
            {
                List<int> users = JsonConvert.DeserializeObject<List<int>>(res);
                if (users.Count > 0)
                {
                    Random r  = new Random();
                    int pos = r.Next(0, users.Count - 1);
                    To = users[pos];
                }
                else
                {
                    To = -1;
                }
            }
        }

        public void GetMessages(object sender)
        {
            string url = settings["appURL"] + "api/message/GetMessagesByKeys/";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["keys"] = authData.userId.ToString() + "_" + To.ToString();
            RequestService.MakeGetRequest(authData, url, parameters, DownloadStringHandler);
            GetMessageCount();
        }

        private void GetProfiles()
        {
            string url = settings["appURL"] + "api/ProfileApi";
            Dictionary<String,String> parameters = new Dictionary<string, string>();
            parameters["userId"] = authData.userId.ToString();
            parameters["token"] = authData.token.ToString();
            RequestService.MakeGetRequest(authData, url, parameters, SelfProfileHandler);
            var parameters1 = new Dictionary<string, string>();
            parameters1["userId"] = authData.userId.ToString();
            parameters1["token"] = authData.token.ToString();
            parameters1["targetId"] = To.ToString();
            RequestService.MakeGetRequest(authData, url, parameters1, TargetProfileHandler);
        }
        
        private void SelfProfileHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            user = JsonConvert.DeserializeObject<Profile>(e.Result);
        }

        private void TargetProfileHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            target = JsonConvert.DeserializeObject<Profile>(e.Result);
        }

        private void MessageToSend_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        public void DownloadStringHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            string res = e.Result;
            List<MessageModel> messages = JsonConvert.DeserializeObject<List<MessageModel>>(res);
            string mes = "";
            if (!profileEnabled || user == null || target == null)
            {
                foreach (MessageModel messageModel in messages)
                {
                    mes += messageModel.From + ": " + messageModel.Message + "\r\n";
                }
            }
            else
            {

                foreach (MessageModel messageModel in messages)
                {
                    if (messageModel.From == authData.userId)
                    {
                        mes += user.FirstName;
                    }
                    else
                    {
                        mes += target.FirstName;
                    }
                    mes += ": " + messageModel.Message + "\r\n";
                }
            }

            Dispatcher.BeginInvoke(() => ChangeMessageText(mes));
            
        }

        public void ChangeMessageText(string text)
        {
            AllMessages.Text = text;
        }

        private void MessageToSend_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageToSend.Text.Equals("Put Your message here"))
                MessageToSend.Text = "";
        }

        private void SendButon_Click(object sender, RoutedEventArgs e)
        {
            if (MessageToSend.Text != "")
            {
                string url = settings["appURL"]+"api/message/GetSendMessage/";
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["from"] = authData.userId.ToString();
                parameters["to"] = To.ToString();
                parameters["messag"] = MessageToSend.Text;
                RequestService.MakeGetRequest(authData, url, parameters, SendStringHandler);

            }
        }

        public void ChangeSendText(string text)
        {
            MessageToSend.Text = text;
        }

        public void SendStringHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            timer = new Timer(GetMessages, null, 0, 2000);
            string res = e.Result;
            Dispatcher.BeginInvoke(() => ChangeSendText(""));
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            UserProfile.authData = authData;
            UserProfile.profileId = To;
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/UserProfile.xaml", UriKind.Relative)));
        }
    }
}