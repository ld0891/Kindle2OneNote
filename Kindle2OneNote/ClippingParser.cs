using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Kindle2OneNote
{
    public struct Clipping
    {
        public string Title;
        public string Author;
        public uint Page;
        public uint LocationFrom;
        public uint LocationTo;
        public DateTime AddTime;
        public string content;
    }

    public sealed class ClippingParser
    {
        private static volatile ClippingParser instance = null;
        private static object syncRoot = new Object();

        private ClippingParser() { }

        public static ClippingParser Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ClippingParser();
                    }
                }

                return instance;
            }
        }

        public List<Clipping> Parse(string fileContent)
        {
            /*
            string[] clippingSeparator = { "==========" };
            string[] contentSeparator = { "\r\n\r\n" };
            string text = await Windows.Storage.FileIO.ReadTextAsync(file);
            string[] clippingTexts = text.Split(clippingSeparator, StringSplitOptions.RemoveEmptyEntries);

            string[] clippings;
            string metaInfo;
            string highlight;
            string pat = @"(?<title>.*?) \((?<author>.*?)\)\r\n- .*?(?<page>\d+) \| Location (?<from>\d+)-(?<to>\d+) \| Added on (?<time>.*)";
            Regex regex = new Regex(pat);

            foreach (string content in clippingTexts)
            {
                clippings = content.Split(contentSeparator, StringSplitOptions.RemoveEmptyEntries);
                metaInfo = clippings[0];
                highlight = clippings[1];

                Match match = regex.Match(content);
                if (!match.Success)
                {
                    return;
                }

                string title = match.Groups["title"].Value;
                string author = match.Groups["author"].Value;
                uint page = Convert.ToUInt32(match.Groups["page"].Value);
                uint locationFrom = Convert.ToUInt32(match.Groups["from"].Value);
                uint locationTo = Convert.ToUInt32(match.Groups["to"].Value);
                DateTime date = DateTime.Parse(match.Groups["time"].Value);
            }
            */

            List<Clipping> clippings = new List<Clipping>();
            return clippings;
        }
    }
}
