using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

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
        public string Content;
    }

    public static class Constants
    {
        public const string pattern = 
            @"(?<title>.*?) \((?<author>.*?)\)\r\n- .*?(?<page>\d+) \| Location (?<from>\d+)-(?<to>\d+) \| Added on (?<time>.*)\r\n\r\n(?<content>.*)\r\n={10}";
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
            Clipping clip;
            Regex regex = new Regex(Constants.pattern);
            Match match = regex.Match(fileContent);
            List<Clipping> clippings = new List<Clipping>();

            while (match.Success)
            {
                clip.Title = match.Groups["title"].Value;
                clip.Author = match.Groups["author"].Value;
                clip.Page = Convert.ToUInt32(match.Groups["page"].Value);
                clip.LocationFrom = Convert.ToUInt32(match.Groups["from"].Value);
                clip.LocationTo = Convert.ToUInt32(match.Groups["to"].Value);
                clip.AddTime = DateTime.Parse(match.Groups["time"].Value);
                clip.Content = match.Groups["content"].Value;
                clippings.Add(clip);

                match = match.NextMatch();
            }

            return clippings;
        }
    }
}
