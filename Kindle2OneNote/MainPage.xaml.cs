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
        private Presenter _presenter = new Presenter();
        public MainPage()
        {
            this.InitializeComponent();
            
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size(480, 300);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;

            this.DataContext = _presenter;
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

        private void DisplayNotebooks()
        {
            notebookComboBox.ItemsSource = OneNote.Instance.Notebooks;
            notebookComboBox.DisplayMemberPath = "Name";

            foreach (Notebook book in OneNote.Instance.Notebooks)
            {
                if (book.Selected)
                {
                    notebookComboBox.SelectedItem = book;
                    return;
                }
            }
            notebookComboBox.SelectedIndex = 0;
        }

        private void notebookComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var notebooksComboBox = sender as ComboBox;
            var notebook = notebooksComboBox.SelectedItem as Notebook;
            if (notebook == null)
            {
                return;
            }

            sectionComboBox.ItemsSource = notebook.Sections;
            sectionComboBox.DisplayMemberPath = "Name";

            foreach (Section section in notebook.Sections)
            {
                if (section.Selected)
                {
                    sectionComboBox.SelectedItem = section;
                    return;
                }
            }
            sectionComboBox.SelectedIndex = 0;
        }

        private void sectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sectionComboBox = sender as ComboBox;
            var section = sectionComboBox.SelectedItem as Section;
            if (section == null)
            {
                return;
            }

            OneNote.Instance.TargetSectionId = section.Id;
            RefreshSelectFileButtonStatus();
        }

        private void notebookComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            notebookComboBox.ItemsSource = OneNote.Instance.Notebooks;
            notebookComboBox.DisplayMemberPath = "Name";
            notebookComboBox.SelectedIndex = -1;
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
                await OneNote.Instance.LoadNotebooks();
                DisplayNotebooks();
                notebookRing.IsActive = false;
                sectionRing.IsActive = false;
            }

            RefreshSelectFileButtonStatus();
        }

        private void RefreshSelectFileButtonStatus()
        {
            if (!Account.IsSignedIn() ||
                OneNote.Instance.TargetSectionId == null ||
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
