using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kindle2OneNote
{
    public sealed class FileManager
    {
        private static volatile FileManager instance = null;
        private static object syncRoot = new Object();

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

        public async Task<bool> MoveFileToFolder(string filePath, string folderPath)
        {
            Windows.Storage.StorageFile file;
            Windows.Storage.StorageFolder folder;
            try
            {
                Task<Windows.Storage.StorageFile> fileTask = Windows.Storage.StorageFile.GetFileFromPathAsync(filePath).AsTask();
                Task<Windows.Storage.StorageFolder> folderTask = Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath).AsTask();
                file = await fileTask;
                folder = await folderTask;
            }
            catch
            {
                return false;
            }
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
    }
}
