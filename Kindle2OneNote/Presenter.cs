using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kindle2OneNote
{
    public sealed class Presenter: ObservableObject
    {
        private string _backupFolderPath;
        private string _userStatusText;
        private string _signInButtonText;
        private ObservableCollection<Notebook> _notebooks;
        private Notebook _selectedBook;

        private static volatile Presenter instance = null;
        private static object syncRoot = new Object();

        private Presenter()
        {
            RefreshTexts();
        }

        public static Presenter Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Presenter();
                    }
                }

                return instance;
            }
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

        public IEnumerable<Notebook> Notebooks
        {
            get { return _notebooks; }
            set
            {
                _notebooks = new ObservableCollection<Notebook>(value);
                RaisePropertyChangedEvent("Notebooks");
            }
        }

        public Notebook SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                _selectedBook = value;
                RaisePropertyChangedEvent("SelectedBook");
            }
        }

        public ICommand SignInOrOutCommand
        {
            get { return new DelegateCommand(SignInOrOut); }
        }

        public void OnSignInComplete()
        {
            RefreshTexts();
            RefreshNotebook();
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
                _notebooks.Clear();
                RefreshTexts();
            }
            else
            {
                /*
                notebookRing.IsActive = true;
                sectionRing.IsActive = true;
                */
                Account.SignIn();
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

        private void RefreshTexts()
        {
            bool signedIn = Account.IsSignedIn();
            SignInButtonText = signedIn ? "Sign Out" : "Sign In";
            UserStatusText = signedIn ? "Signed in" : "Not set yet";

            BackupFolderPath = FileManager.Instance.GetBackupFolderPath().Result;
        }

        private async void RefreshNotebook()
        {
            Notebooks = await OneNote.Instance.LoadNotebooks();
            if (!Notebooks.Any())
                return;

            foreach (Notebook book in Notebooks)
            {
                if (book.Selected)
                {
                    SelectedBook = book;
                    return;
                }
            }
            SelectedBook = Notebooks.First();
        }
    }
}
