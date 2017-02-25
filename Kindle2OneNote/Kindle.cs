using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

namespace Kindle2OneNote
{
    public sealed class Kindle
    {
        private static volatile Kindle instance = null;
        private static object syncRoot = new Object();

        private static readonly String clippingFilePath = @"documents/My Clippings.txt";

        private Kindle() { }

        public static Kindle Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Kindle();
                    }
                }

                return instance;
            }
        }

        public async Task<StorageFile> GetClippingFile()
        {
            StorageFile file = null;
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            IReadOnlyList<StorageFolder> devices = await externalDevices.GetFoldersAsync();
            foreach (StorageFolder device in devices)
            {
                try
                {
                    file = await device.GetFileAsync(clippingFilePath);
                    break;
                }
                catch
                {
                    file = null;
                    continue;
                }
            }
            return file;
        }
    }
}
