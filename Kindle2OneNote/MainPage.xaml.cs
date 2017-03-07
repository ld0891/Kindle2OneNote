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
using System.Threading.Tasks;


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
            
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size(480, 300);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;

            this.DataContext = Presenter.Instance;
        }

        public void OnUploadStatus(bool isSuccess)
        {
            uploadRing.Visibility = Visibility.Collapsed;
            uploadNotificationTextBlock.Visibility = Visibility.Collapsed;
            Notification.Instance.Show("Success", "Successfully uploaded your clippings");
        }

        public void OnUploadStart()
        {
            uploadRing.Visibility = Visibility.Visible;
            uploadNotificationTextBlock.Visibility = Visibility.Visible;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            uploadRing.IsActive = true;
            uploadRing.Visibility = Visibility.Collapsed;
            uploadNotificationTextBlock.Visibility = Visibility.Collapsed;

            if (Account.IsSignedIn())
            {
                notebookRing.IsActive = true;
                sectionRing.IsActive = true;
                notebookRing.IsActive = false;
                sectionRing.IsActive = false;
            }

            RefreshSelectFileButtonStatus();
        }

        private void RefreshSelectFileButtonStatus()
        {
            if (!Account.IsSignedIn() ||
                FileManager.Instance.GetBackupFolderPath() == null)
            {
                selectFileButton.IsEnabled = false;
            }
            else
            {
                selectFileButton.IsEnabled = true;
            }
        }
    }
}
