using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using _4sqApp.Resources;
using Newtonsoft.Json;
using Newtonsoft;
namespace _4sqApp
{
    public class AuthData
    {
        [JsonProperty("userId")]
        public int userId;
        [JsonProperty("token")]
        public string token;

        public AuthData(int userId, string token)
        {
            this.userId = userId;
            this.token = token;
        }
    }

    

    public partial class MainPage : PhoneApplicationPage
    {
        private bool visited = false;
        private HttpWebResponse webResponse;
        private AuthData authData;
        private void GetAuthData()
        {
            CookieCollection cookies = webbrowser1.GetCookies();
            string cookieString = "";
            foreach (Cookie cookie in cookies)
            {
                cookieString += String.Format("{0}={1};", cookie.Name, cookie.Value);
            }
            WebRequest wr = HttpWebRequest.Create("http://4sqchat.somee.com/FoursquareLogin/AuthInfo");
            wr.Headers["Cookie"] = cookieString;
            wr.BeginGetResponse(new AsyncCallback(FinishRequest), wr);
            
            
        }

        private void FinishRequest(IAsyncResult result)
        {
            webResponse = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
            string res;
            using (Stream stream = webResponse.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                res = sr.ReadToEnd();
            }
            AuthData authData = JsonConvert.DeserializeObject<AuthData>(res);
            Xroads.authData = authData;
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Xroads.xaml", UriKind.Relative)));
            
        }

        // Конструктор
        public MainPage()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings["appURL"] = "http://4sqchat.somee.com/";
            settings.Save();
            InitializeComponent();
            webbrowser1.Source = new Uri("http://4sqchat.somee.com/FoursquareLogin");
            
            // Пример кода для локализации ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void webbrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.ToString() == "http://4sqchat.somee.com/Foursquare")
            {
                GetAuthData();
            }
        }

        // Пример кода для построения локализованной панели ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Установка в качестве ApplicationBar страницы нового экземпляра ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Создание новой кнопки и установка текстового значения равным локализованной строке из AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Создание нового пункта меню с локализованной строкой из AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}