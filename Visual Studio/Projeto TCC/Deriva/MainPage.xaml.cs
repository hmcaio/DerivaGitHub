using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Windows.Foundation;
using Windows.Devices.Geolocation;

using UnityApp = UnityPlayer.UnityApp;
using UnityBridge = WinRTBridge.WinRTBridge;
using Microsoft.Xna.Framework.Media;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Microsoft.Devices;

namespace Deriva
{
	public partial class MainPage : PhoneApplicationPage
	{
		private bool _unityStartedLoading;
		private bool _useLocation;

        private DispatcherTimer extendedSplashTimer;
        private bool isUnityLoaded;

		// Constructor
		public MainPage()
		{
			var bridge = new UnityBridge();
			UnityApp.SetBridge(bridge);
			InitializeComponent();
			bridge.Control = DrawingSurfaceBackground;

            //Ensure we listen to when Unity tells us the game is ready
            StaticInterop.UnityLoaded = OnUnityLoaded;

            //Create extended splash timer
            extendedSplashTimer = new DispatcherTimer();
            extendedSplashTimer.Interval = TimeSpan.FromMilliseconds(100);
            extendedSplashTimer.Tick += ExtendedSplashTimer_Tick;
            extendedSplashTimer.Start();
		}

		private void DrawingSurfaceBackground_Loaded(object sender, RoutedEventArgs e)
		{
            if (!_unityStartedLoading)
            {
                _unityStartedLoading = true;

                UnityApp.SetLoadedCallback(() => { Dispatcher.BeginInvoke(Unity_Loaded); });

                var content = Application.Current.Host.Content;
                var width = (int)Math.Floor(content.ActualWidth * content.ScaleFactor / 100.0 + 0.5);
                var height = (int)Math.Floor(content.ActualHeight * content.ScaleFactor / 100.0 + 0.5);

                UnityApp.SetNativeResolution(width, height);
                UnityApp.SetRenderResolution(width, height);
                UnityPlayer.UnityApp.SetOrientation((int)Orientation);

                DrawingSurfaceBackground.SetBackgroundContentProvider(UnityApp.GetBackgroundContentProvider());
                DrawingSurfaceBackground.SetBackgroundManipulationHandler(UnityApp.GetManipulationHandler());

                if (StaticInterop.IsFirstTime)
                {
                    StaticInterop.SaveSSToLibrary += StaticInterop_SaveSSToLibrary;
                    //StaticInterop.OnPost += StaticInterop_PostScreenshot;
                    //StaticInterop.OnExit += StaticInterop_OnExit;
                    //StaticInterop.OnVibrate += StaticInterop_OnVibrate;

                    StaticInterop.IsFirstTime = false;
                }
            }
		}

        VibrateController vibrator = VibrateController.Default;
        void StaticInterop_OnVibrate(int t)
        {
            vibrator.Start(new TimeSpan(0, 0, 0, 0, t));
        }

        private void StaticInterop_OnExit(object sender, EventArgs e)
        {
            //Unreachable code (?)

            //FBMan.FacebookSessionClient.Logout();  //Does not work/Was not implemented
            //ClearFacebookCookies();
        }

        //private void StaticInterop_PostScreenshot(byte[] bytes)
        //{
        //    FBMan.bytes = bytes;
        //    if (!FBMan.isAuthenticated)
        //    {
        //        Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Pages/FBLoginPage.xaml", UriKind.Relative)));
        //    }
        //    else
        //    {
        //        Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Pages/FBPostPage.xaml", UriKind.Relative)));
        //    }
        //}

        private void StaticInterop_SaveSSToLibrary(object sender, EventArgs e)
        {
            new MediaLibrary().SavePicture(StaticInterop.imageTitle, StaticInterop.bytes);
        }

        /// <summary>
        /// Control the extended splash experience
        /// </summary>
        async void ExtendedSplashTimer_Tick(object sender, object e)
        {
            var increment = extendedSplashTimer.Interval.TotalMilliseconds;
            if (!isUnityLoaded && SplashProgress.Value <= (SplashProgress.Maximum - increment))
            {
                SplashProgress.Value += increment;
            }
            else
            {
                SplashProgress.Value = SplashProgress.Maximum;
                await Task.Delay(1000);  //Force delay so user can see progress bar maxing out very briefly
                RemoveExtendedSplash();
            }
        }

        /// <summary>
        /// Unity has loaded and the game is playable
        /// </summary>
        async void OnUnityLoaded()
        {
            isUnityLoaded = true;
        }

        /// <summary>
        /// Remove the extended splash
        /// </summary>
        private void RemoveExtendedSplash()
        {
            if (extendedSplashTimer != null)
            {
                extendedSplashTimer.Stop();
            }

            if (DrawingSurfaceBackground.Children.Count > 0)
            {
                DrawingSurfaceBackground.Children.Remove(ExtendedSplashGrid);
            }
        }

        private void Unity_Loaded()
        {
            SetupGeolocator();
        }

		private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
		{
			e.Cancel = UnityApp.BackButtonPressed();
		}

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //base.OnBackKeyPress(e);

            e.Cancel = true;  //Suppress default behaviour

            //FBMan.FacebookSessionClient.Logout();  //It reaches this line, but it does not work/Was not implemented (?)

            Application.Current.Terminate();  //Whenever in MainPage and back button pressed, exit application
        }

		private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			UnityApp.SetOrientation((int)e.Orientation);
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!UnityApp.IsLocationEnabled())
                return;
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
                _useLocation = (bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"];
            else
            {
                MessageBoxResult result = MessageBox.Show("Can this application use your location?",
                    "Location Services", MessageBoxButton.OKCancel);
                _useLocation = result == MessageBoxResult.OK;
                IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = _useLocation;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

		private void SetupGeolocator()
        {
            if (!_useLocation)
                return;

            try
            {
				UnityApp.EnableLocationService(true);
                Geolocator geolocator = new Geolocator();
				geolocator.ReportInterval = 5000;
                IAsyncOperation<Geoposition> op = geolocator.GetGeopositionAsync();
                op.Completed += (asyncInfo, asyncStatus) =>
                    {
                        if (asyncStatus == AsyncStatus.Completed)
                        {
                            Geoposition geoposition = asyncInfo.GetResults();
                            UnityApp.SetupGeolocator(geolocator, geoposition);
                        }
                        else
                            UnityApp.SetupGeolocator(null, null);
                    };
            }
            catch (Exception)
            {
                UnityApp.SetupGeolocator(null, null);
            }
        }

        private void ExtendedSplashImage_Loaded(object sender, RoutedEventArgs e)
        {
            SplashProgress.Visibility = System.Windows.Visibility.Visible;
        }
	}
}