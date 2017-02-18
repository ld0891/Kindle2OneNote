using System;
using System.Collections.Generic;

namespace Kindle2OneNote
{
    public sealed class OneNoteClient
    {
        private static volatile OneNoteClient instance = null;
        private static object syncRoot = new Object();

        private OneNoteClient() { }

        public static OneNoteClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new OneNoteClient();
                    }
                }

                return instance;
            }
        }

        public List<string> fetchNotebooks()
        {
            List<string> notebookNames = new List<string>();
            notebookNames.Add("readings");
            notebookNames.Add("writings");
            notebookNames.Add("belongings");
            return notebookNames;
        }
    }
}
