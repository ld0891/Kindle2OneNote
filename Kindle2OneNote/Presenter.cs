using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kindle2OneNote
{
    public class Presenter: ObservableObject
    {
        private string _backupFolderPath;
        private string _userStatusText;
        private string _signInButtonText;

        public Presenter()
        {
            RefreshTexts();
        }

        public string BackupFolderPath
        {
            get { return _backupFolderPath == null ? @"Not set yet" : _backupFolderPath; }
            set
            {
                _backupFolderPath = value;
                RaisePropertyChangedEvent("BackupFolderPath");
            }
        }

        public string UserStatusText
        {
            get { return _userStatusText; }
            set
            {
                _userStatusText = value;
                RaisePropertyChangedEvent("UserStatusText");
            }
        }

        public string SignInButtonText
        {
            get { return _signInButtonText; }
            set
            {
                _signInButtonText = value;
                RaisePropertyChangedEvent("SignInButtonText");
            }
        }

        public ICommand SignInOrOutCommand
        {
            get { return new DelegateCommand(SignInOrOut); }
        }

        private async void SignInOrOut()
        {
            if (Account.IsSignedIn())
            {
                await Account.SignOut();
                /*
                notebookComboBox.ItemsSource = null;
                sectionComboBox.ItemsSource = null;
                */
                OneNote.Instance.Reset();
                FileManager.Instance.Reset();
            }
            else
            {
                /*
                notebookRing.IsActive = true;
                sectionRing.IsActive = true;
                */
                Account.SignIn();
            }

            RefreshTexts();
        }

        public ICommand SelectBackupFolderCommand
        {
            get { return new DelegateCommand(SelectBackupFolder); }
        }

        private async void SelectBackupFolder()
        {
            BackupFolderPath = await FileManager.Instance.SelectFolder();
        }

        public ICommand SelectClippingFileCommand
        {
            get { return new DelegateCommand(SelectClippingFile); }
        }

        private async void SelectClippingFile()
        {
            Windows.Storage.StorageFile clippingFile = await FileManager.Instance.SelectFile();
            Kindle.Instance.SendClippingsToOneNote(clippingFile);
        }

        public ICommand SetupAutoPlayCommand
        {
            get { return new DelegateCommand(SetupAutoPlay); }
        }

        private async void SetupAutoPlay()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:autoplay"));
        }

        private void RefreshTexts()
        {
            bool signedIn = Account.IsSignedIn();
            SignInButtonText = signedIn ? "Sign Out" : "Sign In";
            UserStatusText = signedIn ? "Signed in" : "Not set yet";

            BackupFolderPath = FileManager.Instance.GetBackupFolderPath().Result;
        }
    }
}
