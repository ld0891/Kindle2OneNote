﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace Kindle2OneNote
{
    public static class NoteRequest
    {
        public static readonly string dataId = @"_clippings_by_Kindle2OneNote";
        private static readonly string timeFormat = @"yyyy/MM/ddTHH:mm:sszzz";
        private static readonly string styleKey = @"style";
        private static readonly string metaStyle = @"font-size:9pt;color:#7f7f7f;margin-top:0pt;margin-bottom:0pt";
        private static readonly string contentStyle = @"font-size:12pt;color:black;font-style:italic;margin-top:0pt;margin-bottom:12pt";

        public static string CreatePage(BookWithClippings book)
        {
            if (!book.Clippings.Any())
            {
                return null;
            }

            string pageName = book.GeneratePageName();
            string content = UpdatePage(book.Clippings);

            StringBuilder str = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.OmitXmlDeclaration = true;
            using (var xml = XmlWriter.Create(str, settings))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("html");
                xml.WriteStartElement("head");
                xml.WriteElementString("title", book.GeneratePageName());
                xml.WriteStartElement("meta");
                xml.WriteAttributeString("name", "created");
                xml.WriteAttributeString("content", DateTime.Now.ToString(timeFormat));
                xml.WriteEndElement();
                xml.WriteEndElement();
                xml.WriteStartElement("body");
                xml.WriteStartElement("div");
                xml.WriteAttributeString("data-id", dataId);
                xml.WriteRaw(content);
                xml.WriteEndElement();
                xml.WriteEndElement();
                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
            return str.ToString();
        }

        public static string UpdatePage(List<Clipping> clippings)
        {
            if (!clippings.Any())
            {
                return null;
            }

            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string metaFormat = loader.GetString("ClipMeta");
            StringBuilder str = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            using (var xml = XmlWriter.Create(str, settings))
            {
                string meta = "";
                foreach (Clipping clip in clippings)
                {
                    meta = String.Format(metaFormat, clip.Page, clip.LocationFrom, clip.LocationTo, clip.AddTime.ToString("F"));
                    xml.WriteStartElement("p");
                    xml.WriteAttributeString(styleKey, metaStyle);
                    xml.WriteString(meta);
                    xml.WriteEndElement();
                    xml.WriteStartElement("p");

                    xml.WriteAttributeString(styleKey, contentStyle);
                    xml.WriteString(clip.Content);
                    xml.WriteEndElement();
                }
            }
            return str.ToString();
        }
    }
}
