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

        public void BackupClippings()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            /*
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            }
            */
            return true;
        }

        public string ReadFileContent()
        {
            return "";
        }

        private bool KindleConnected()
        {
            return true;
        }

        private bool ClippingsExist()
        {
            return true;
        }
        
        private string ClippingsFilePath()
        {
            return "";
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

        private async Task<string> ReadFile(string filePath)
        {
            Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
            string content = await Windows.Storage.FileIO.ReadTextAsync(file);
            return content;
        }
    }
}
