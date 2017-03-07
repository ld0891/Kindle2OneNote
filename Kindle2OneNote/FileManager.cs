using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

namespace Kindle2OneNote
{
    public sealed class FileManager
    {
        private static volatile FileManager instance = null;
        private static object syncRoot = new Object();
        private static readonly string folderToken = @"_backup_folder_Kindle2OneNote";

        private FileManager() { }

        public static FileManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new FileManager();
                    }
                }

                return instance;
            }
        }

        public async Task<string> GetBackupFolderPath()
        {
            if (Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(folderToken))
            {
                StorageFolder folder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                return folder.Path;
            }

            return null;
        }

        public async Task<string> SelectFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
            {
                return null;
            }

            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(folderToken, folder);
            return folder.Path;
        }

        public async Task<StorageFile> SelectFile()
        {
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add(".txt");
            StorageFile file = await filePicker.PickSingleFileAsync();
            return file;
        }

        public void Reset()
        {
            if (Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(folderToken))
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(folderToken);
            }
        }

        public async Task<bool> BackupFile(string filePath)
        {
            StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath).AsTask();
            bool result = await BackupFile(file);
            return result;
        }

        public async Task<bool> BackupFile(StorageFile file)
        {
            StorageFolder folder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            if (file == null || folder == null)
                return false;

            string backupName = string.Concat(
                file.DisplayName,
                @"_",
                DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                file.FileType);

            try
            {
                await file.CopyAsync(folder, backupName);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<string> ReadFileContent(string filePath)
        {
            if (!await FileExists(filePath))
            {
                return "";
            }

            Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
            string content = await Windows.Storage.FileIO.ReadTextAsync(file);
            return content;
        }

        public async Task<string> ReadFileContent(StorageFile file)
        {
            if (file == null || !file.IsAvailable)
            {
                return null;
            }
            
            string content = await FileIO.ReadTextAsync(file);
            return content;
        }

        private async Task<bool> FileExists(string filePath)
        {
            try
            {
                Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async void DeleteFile(StorageFile file)
        {
            if (file == null)
                return;
            await file.DeleteAsync();
        }
    }
}
