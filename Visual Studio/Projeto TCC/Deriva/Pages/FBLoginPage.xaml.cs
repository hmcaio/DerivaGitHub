using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Facebook.Client;

namespace Deriva.Pages
{
    public partial class FBLoginPage : PhoneApplicationPage
    {
        public FBLoginPage()
        {
            InitializeComponent();

            this.Loaded += FBLoginPage_Loaded;
        }

        async void FBLoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!FBMan.isAuthenticated)
            {
                //FBMan.isAuthenticated = true;
                await Authenticate();
            }
        }

        private FacebookSession session;
        async Task Authenticate()
        {
            try
            {
                session = await FBMan.FacebookSessionClient.LoginAsync("publish_actions");
                FBMan.AccessToken = session.AccessToken;
                FBMan.FacebookId = session.FacebookId;

                FBMan.isAuthenticated = true;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    NavigationService.Navigate(new Uri("/Pages/FBPostPage.xaml", UriKind.Relative)));
            }
            catch (InvalidOperationException e)
            {
                if (!e.Message.Contains("Login in progress"))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (MessageBox.Show("Login failed! Exception details:\n" + e.Message) == MessageBoxResult.OK)
                            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    });
                }
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //base.OnBackKeyPress(e);

            e.Cancel = true;  //Suppress default behaviour

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            });
        }
    }
}