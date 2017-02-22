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
            StorageFolder folder;
            try
            {
                folder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            }
            catch
            {
                return null;
            }
            if (folder == null)
            {
                return null;
            }

            return folder.Path;
        }

        public void OnNewFolderSelected(StorageFolder folder)
        {
            if (folder == null)
            {
                return;
            }

            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(folderToken, folder);
        }

        public async Task<bool> BackupFile(string filePath)
        {
            StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath).AsTask();
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
                await file.MoveAsync(folder, backupName);
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

        public async void DeleteFile(string filePath)
        {
            StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath).AsTask();
            if (file == null)
                return;
            await file.DeleteAsync();
        }
    }
}
