using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Kindle2OneNote
{
    class NotePage : IEquatable<NotePage>
    {
        private const string idKey = "id";
        private const string nameKey = "title";

        public string Id { get; set; }
        public string Name { get; set; }

        public NotePage()
        {
            Id = "";
            Name = "";
        }

        public NotePage(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            Id = jsonObject.GetNamedString(idKey, "");
            Name = jsonObject.GetNamedString(nameKey, "");
        }

        public bool Equals(NotePage other)
        {
            if (other == null)
                return false;

            return String.Equals(this.Name, other.Name);
        }
    }
}
