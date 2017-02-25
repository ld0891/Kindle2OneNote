using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Kindle2OneNote
{
    public class Notebook : IEquatable<Notebook>
    {
        private const string idKey = "id";
        private const string nameKey = "name";
        
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public List<Section> Sections { get; set; }

        public Notebook()
        {
            Id = "";
            Name = "";
            Selected = false;
            Sections = new List<Section>();
        }

        public Notebook(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            Id = jsonObject.GetNamedString(idKey, "");
            Name = jsonObject.GetNamedString(nameKey, "");
        }

        public bool Equals(Notebook other)
        {
            if (other == null)
                return false;

            return String.Equals(this.Id, other.Id);
        }
    }
}
