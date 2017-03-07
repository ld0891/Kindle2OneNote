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

        public ICommand SelectBackupFolderCommand
        {
            get { return new DelegateCommand(SelectBackupFolder); }
        }

        private async void SelectBackupFolder()
        {
            BackupFolderPath = await FileManager.Instance.SelectBackupFolder();
        }
    }
}
