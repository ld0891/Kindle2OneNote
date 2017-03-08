using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace Kindle2OneNote
{
    public sealed class Presenter: ObservableObject
    {
        private string _backupFolderPath;
        private string _userStatusText;
        private string _signInButtonText;
        private ObservableCollection<Notebook> _notebooks;
        private Notebook _selectedBook;
        private ObservableCollection<Section> _sections;
        private Section _selectedSection;

        private static readonly string sectionKey = @"TargetSectionID";
        private static volatile Presenter instance = null;
        private static object syncRoot = new Object();

        private Presenter()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(sectionKey))
            {
                _selectedSection = new Section();
                _selectedSection.Id = ApplicationData.Current.LocalSettings.Values[sectionKey] as String;
            }

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
                if (_selectedBook == null)
                    return;

                Sections = new ObservableCollection<Section>(_selectedBook.Sections);
                if (!Sections.Any())
                    return;
                foreach (Section section in Sections)
                {
                    if (section.Selected)
                    {
                        SelectedSection = section;
                        return;
                    }
                }
                SelectedSection = Sections.First();
            }
        }

        public IEnumerable<Section> Sections
        {
            get { return _sections; }
            set
            {
                _sections = new ObservableCollection<Section>(value);
                RaisePropertyChangedEvent("Sections");
            }
        }

        public Section SelectedSection
        {
            get { return _selectedSection; }
            set
            {
                _selectedSection = value;
                RaisePropertyChangedEvent("SelectedSection");
                if (_selectedSection != null)
                    ApplicationData.Current.LocalSettings.Values[sectionKey] = _selectedSection.Id;
            }
        }

        public ICommand SignInOrOutCommand
        {
            get { return new DelegateCommand(SignInOrOut); }
        }

        public void OnSignInComplete(bool Success)
        {
            RefreshTexts();
            RefreshNotebook();
        }

        private async void SignInOrOut()
        {
            if (Account.IsSignedIn())
            {
                await Account.SignOut();
                FileManager.Instance.Reset();
                _notebooks?.Clear();
                _selectedBook = null;
                RefreshTexts();
            }
            else
            {
                Account.SignIn(OnSignInComplete);
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
            SendClippingsToOneNote(clippingFile);
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
            List<Notebook> notebooks = await OneNote.Instance.LoadNotebooks();
            MarkSelectedNotebookAndSection(notebooks);
            Notebooks = notebooks;
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
        
        private void MarkSelectedNotebookAndSection(List<Notebook> notebooks)
        {
            if (SelectedSection == null || !notebooks.Any())
            {
                return;
            }

            foreach (Notebook book in notebooks)
            {
                foreach (Section section in book.Sections)
                {
                    if (section.Id == SelectedSection.Id)
                    {
                        section.Selected = true;
                        book.Selected = true;
                        return;
                    }
                }
            }
        }

        private async void SendClippingsToOneNote(StorageFile file)
        {
            if (file == null)
                return;

            string fileContent = await FileManager.Instance.ReadFileContent(file);
            List<BookWithClippings> books = ClippingParser.Instance.Parse(fileContent);
            OneNote.Instance.UploadClippingsToSection(books, SelectedSection);
            if (await FileManager.Instance.BackupFile(file))
            {
                FileManager.Instance.DeleteFile(file);
            }
        }
        
        public async void OnNewDeviceConnected()
        {
            StorageFile file = await Kindle.Instance.GetClippingFile();
            if (file == null)
            {
                Notification.Instance.Show("Error", "No kindle or clipping file found.");
                return;
            }

            SendClippingsToOneNote(file);
        }
    }
}
