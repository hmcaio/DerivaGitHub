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
using Facebook;

namespace Deriva.Pages
{
    public partial class FBPostPage : PhoneApplicationPage
    {
        public FBPostPage()
        {
            InitializeComponent();
        }

        async Task PostScreenshot(byte[] bytes, string message)
        {
            FacebookClient fb = new Facebook.FacebookClient(FBMan.AccessToken);
            fb.PostCompleted += fb_PostCompleted;

            var fbUpl = new FacebookMediaObject
            {
                FileName = "UploadedImage.png",
                ContentType = "image/png" /*"image/jpg"*/
            };

            fbUpl.SetValue(bytes);

            var parameters = new Dictionary<string, object>();
            parameters["message"] = message;
            parameters["file"] = fbUpl;

            try
            {
                await fb.PostTaskAsync("me/photos", parameters);
                //await fb.PostTaskAsync("Deriva/feed", parameters);  //Trying to post in fan page. It didn't work...
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("(OAuthException - #190)"))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //Token has expired. Need to ask the user to get the token again before continuing
                        FBMan.isAuthenticated = false;
                        if (MessageBox.Show(ex.Message, "#190", MessageBoxButton.OK) == MessageBoxResult.OK)
                            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    });
                }
                else if (ex is FacebookOAuthException)
                {
                    // oauth exception occurred
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (MessageBox.Show("Authentication error. Details:\n" + ex.Message, "Error", MessageBoxButton.OK) == MessageBoxResult.OK)
                            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    });
                }
                else
                {
                    // non-facebook exception such as no internet connection.
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (MessageBox.Show("An error occured. Details:\n" + ex.Message, "Error", MessageBoxButton.OK) == MessageBoxResult.OK)
                            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    });
                }
            }
        }

        void fb_PostCompleted(object sender, FacebookApiEventArgs args)
        {
            if (args.Cancelled)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (MessageBox.Show("Operation cancelled.", "Cancelled", MessageBoxButton.OK) == MessageBoxResult.OK)
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
            }
            else if (args.Error != null)
            {
                //Nothing (?)
                return;
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Successfully posted!");
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //base.OnBackKeyPress(e);

            e.Cancel = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            });
        }

        private async void btnPost_Click(object sender, RoutedEventArgs e)
        {            
            //Disable button
            btnPost.IsEnabled = false;

            //TODO: Validate message

            await PostScreenshot(FBMan.bytes, txtMessage.Text);
        }
    }
}