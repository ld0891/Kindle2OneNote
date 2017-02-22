using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return this.Title.Equals(other.Title) && this.Author.Equals(other.Author);
        }
    }
}
