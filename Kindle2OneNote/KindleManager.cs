using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kindle2OneNote
{
    public sealed class KindleManager
    {
        private static volatile KindleManager instance = null;
        private static object syncRoot = new Object();

        private KindleManager() { }

        public static KindleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new KindleManager();
                    }
                }

                return instance;
            }
        }

        public static bool IsKindleConnected()
        {
            return true;
        }

        public static string GetClippingFilePath()
        {
            return @"documents/My Clippings.txt";
        }
    }
}
