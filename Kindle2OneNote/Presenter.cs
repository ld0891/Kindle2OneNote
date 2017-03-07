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

        public string BackupFolderPath
        {
            get
            {
                return _backupFolderPath == null ? @"Not set yet" : _backupFolderPath;
            }
            set
            {
                _backupFolderPath = value;
                RaisePropertyChangedEvent("BackupFolderPath");
            }
        }

        public string UserStatus
        {
            get
            {
                return Account.IsSignedIn() ? @"Signed In" : @"Not set yet";
            }
        }

        public string SignInButtonText
        {
            get
            {
                return Account.IsSignedIn() ? @"Sign Out" : @"Sign In";
            }
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
    }
}
