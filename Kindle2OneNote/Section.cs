﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Kindle2OneNote
{
    public class Section
    {
        private const string idKey = "id";
        private const string nameKey = "name";
        private const string parentKey = "parentNotebook";

        public string Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public Notebook parent { get; set; }

        public Section()
        {
            Id = "";
            Name = "";
            Selected = false;
        }

        public Section(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            Id = jsonObject.GetNamedString(idKey, "");
            Name = jsonObject.GetNamedString(nameKey, "");

            string parentString = jsonObject.GetNamedObject(parentKey).ToString();
            parent = new Notebook(parentString);
        }
    }
}
