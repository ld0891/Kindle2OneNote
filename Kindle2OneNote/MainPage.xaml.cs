using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.System;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Security.Credentials;
using Windows.Storage;
using System.Threading.Tasks;
using System.Globalization;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Kindle2OneNote
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static readonly string signInText = "Sign In";
        private static readonly string signOutText = "Sign Out";

        public MainPage()
        {
            this.InitializeComponent();
            
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size(400, 600);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        public void OnSignInStatus(bool isSuccess)
        {
            if (isSuccess)
            {
                signInButton.Content = signOutText;
            }
            else
            {
                signInButton.Content = signInText;
            }
        }

        private void signInButton_Loaded(object sender, RoutedEventArgs e)
        {
            var signInButton = sender as Button;
            if (!OneNote.Instance.IsSignedIn())
            {
                signInButton.Content = signInText;
            }
            else
            {
                signInButton.Content = signOutText;
            }
        }

        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            if (OneNote.Instance.IsSignedIn())
            {
                OneNote.Instance.SignOut();
            }
            else
            {
                OneNote.Instance.SignIn();
            }
        }

        private async void selectButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }

            FileManager.Instance.OnNewFolderSelected(folder);
            StorageFile file = await folder.GetFileAsync(@"My Clippings.txt");
            string fileContent = await Windows.Storage.FileIO.ReadTextAsync(file);
        }
    }
}
