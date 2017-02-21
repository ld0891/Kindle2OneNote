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
        public uint Page { get; set; }
        public uint LocationFrom { get; set; }
        public uint LocationTo { get; set; }
        public DateTime AddTime { get; set; }
        public string Content { get; set; }
    }

    public class BookWithClippings : IEquatable<BookWithClippings>
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public List<Clipping> Clippings { get; set; }

        public BookWithClippings(string title, string author)
        {
            Title = title;
            Author = author;
            Clippings = new List<Clipping>();
        }

        public string GeneratePageName()
        {
            return String.Concat(Title, " by ", Author);
        }

        public bool Equals(BookWithClippings other)
        {
            if (other == null)
                return false;
            
            string thisTitle = this.Title;
            string otherTitle = other.Title;
            bool titleMatch = this.Title.Equals(other.Title);
            bool authorMatch = String.Equals(this.Author, other.Author);

            return titleMatch && authorMatch;
        }
    }

    public sealed class ClippingParser
    {
        private static volatile ClippingParser instance = null;
        private static object syncRoot = new Object();

        private static readonly int notFound = -1;
        private static readonly char[] byteOrderMark = new char[] { '\uFEFF' };
        private static readonly string pattern =
    @"(?<title>.*) \((?<author>.*?)\)\r\n- .*?(?<page>\d+) \| Location (?<from>\d+)-(?<to>\d+) \| Added on (?<time>.*)\r\n\r\n(?<content>.*)\r\n={10}";

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

        public List<BookWithClippings> Parse(string fileContent)
        {
            int index = notFound;
            string title = "";
            string author = "";
            Clipping clip = new Clipping();
            BookWithClippings book = null;
            List<BookWithClippings> books = new List<BookWithClippings>();

            Regex regex = new Regex(pattern);
            Match match = regex.Match(fileContent);
            while (match.Success)
            {
                clip.Page = Convert.ToUInt32(match.Groups["page"].Value);
                clip.LocationFrom = Convert.ToUInt32(match.Groups["from"].Value);
                clip.LocationTo = Convert.ToUInt32(match.Groups["to"].Value);
                clip.AddTime = DateTime.Parse(match.Groups["time"].Value);
                clip.Content = match.Groups["content"].Value;
                
                title = match.Groups["title"].Value.TrimStart(byteOrderMark);
                byte[] ttBytes = Encoding.ASCII.GetBytes(title);

                author = match.Groups["author"].Value;
                book = new BookWithClippings(title, author);
                index = books.IndexOf(book);
                if (index == notFound)
                {
                    book.Clippings.Add(clip);
                    books.Add(book);
                }
                else
                {
                    books[index].Clippings.Add(clip);
                }
                match = match.NextMatch();
            }

            return books;
        }
    }
}
