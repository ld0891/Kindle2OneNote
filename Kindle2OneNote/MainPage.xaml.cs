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

using Windows.Storage;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Kindle2OneNote
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
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
                userText.Text = "Signed in";
                signInButton.Content = "Sign Out";
                notebookRing.IsActive = true;
                sectionRing.IsActive = true;
            }
            else
            {
                userText.Text = "Not set yet";
                signInButton.Content = "Sign In";
            }
        }

        private void signInButton_Loaded(object sender, RoutedEventArgs e)
        {
            var signInButton = sender as Button;
            if (!OneNote.Instance.IsSignedIn())
            {
                signInButton.Content = "Sign In";
            }
            else
            {
                signInButton.Content = "Sign Out";
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

        private async void backupFolderText_Loaded(object sender, RoutedEventArgs e)
        {
            var folderTextBlock = sender as TextBlock;
            string folderPath = await FileManager.Instance.GetBackupFolderPath();
            if (folderPath == null)
            {
                folderTextBlock.Text = "Not set yet";
            }
            else
            {
                folderTextBlock.Text = folderPath;
            }
        }

        private void userText_Loaded(object sender, RoutedEventArgs e)
        {
            var userText = sender as TextBlock;
            if (OneNote.Instance.IsSignedIn())
            {
                userText.Text = "Signed in";
            }
            else
            {
                userText.Text = "Not set yet";
            }
        }
    }
}
