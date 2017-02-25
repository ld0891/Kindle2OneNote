using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.Web.Http.Headers;
using Windows.System;
using Windows.Data.Json;
using Windows.Web.Http;

using Windows.Storage.Streams;


namespace Kindle2OneNote
{
    public sealed class OneNote
    {
        private static volatile OneNote instance = null;
        private static object syncRoot = new Object();

        private static HttpClient client;
        public List<Notebook> Notebooks { get; private set; }
        public string SectionId { get; set; }

        private static readonly int notFound = -1;
        private static readonly string valueKey = @"value";
        private static readonly Uri baseUri = new Uri(@"https://www.onenote.com/api/v1.0/me/notes/");

        private OneNote()
        {
            Notebooks = new List<Notebook>();
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        public static OneNote Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new OneNote();
                        }
                    }
                }

                return instance;
            }
        }

        public async Task LoadNotebooks()
        {
            string response = await QuerySections();
            List<Section> sections = ParseSectionResponse(response);
            Notebooks = BuildNotebooksFromSections(sections);
        }

        public async void UploadClippingsToSection(string sectionId, List<BookWithClippings> books)
        {
            if (!books.Any())
            {
                return;
            }

            bool bFound = false;
            string targetName = "";
            List<NotePage> pages = await QueryPagesInSection(sectionId);
            foreach (BookWithClippings book in books)
            {
                bFound = false;
                targetName = book.GeneratePageName();
                foreach (NotePage page in pages)
                {
                    if (String.Equals(page.Name, targetName))
                    {
                        bFound = true;
                        AppendClippingsToPage(page.Id, book.Clippings);
                        break;
                    }
                }

                if (!bFound)
                {
                    CreateNewPageInSection(sectionId, book);
                }
            }
        }

        private async Task<string> QuerySections()
        {
            var queryApi = new Uri(baseUri, @"sections");
            string token = await Account.GetTokenSilentlyAsync();
            if (token == null)
            {
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);
            var infoResult = await client.GetAsync(queryApi);
            string content = await infoResult.Content.ReadAsStringAsync();
            return content;
        }

        private List<Section> ParseSectionResponse(string response)
        {
            JsonObject jsonObject;
            if (!JsonObject.TryParse(response, out jsonObject))
            {
                return null;
            }

            var sections = new List<Section>();
            foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(valueKey, new JsonArray()))
            {
                if (jsonValue.ValueType == JsonValueType.Object)
                {
                    var section = new Section(jsonValue.GetObject().ToString());
                    sections.Add(section);
                }
            }
            return sections;
        }

        private List<Notebook> BuildNotebooksFromSections(List<Section> sections)
        {
            if (sections == null)
            {
                return null;
            }

            int index = 0;
            var notebooks = new List<Notebook>();
            foreach (Section section in sections)
            {
                index = notebooks.IndexOf(section.parent);
                if (index == notFound)
                {
                    section.parent.Sections.Add(section);
                    notebooks.Add(section.parent);
                }
                else
                {
                    notebooks[index].Sections.Add(section);
                }
            }
            return notebooks;
        }

        private async Task<List<NotePage>> QueryPagesInSection(string sectionId)
        {
            string token = await Account.GetTokenSilentlyAsync();
            var queryApi = new Uri(baseUri, String.Format("sections/{0}/pages", sectionId));
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            var infoResult = await client.GetAsync(queryApi);
            string content = await infoResult.Content.ReadAsStringAsync();
            return ParseNotePageResponse(content);
        }

        private List<NotePage> ParseNotePageResponse(string response)
        {
            NotePage notePage = null;
            var notePages = new List<NotePage>();
            var jsonObject = JsonObject.Parse(response);

            foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(valueKey, new JsonArray()))
            {
                if (jsonValue.ValueType == JsonValueType.Object)
                {
                    notePage = new NotePage(jsonValue.GetObject().ToString());
                    notePages.Add(notePage);
                }
            }
            return notePages;
        }

        private async void CreateNewPageInSection(string sectionId, BookWithClippings book)
        {
            string token = await Account.GetTokenSilentlyAsync();
            string requestBody = NoteRequest.CreatePage(book);
            var createApi = new Uri(baseUri, String.Format("sections/{0}/pages", sectionId));
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);
            HttpStringContent content = new HttpStringContent(requestBody, UnicodeEncoding.Utf8, @"application/xhtml+xml");

            HttpResponseMessage httpResponse = await client.PostAsync(createApi, content);
            HttpStatusCode code = httpResponse.StatusCode;
            string resp = await httpResponse.Content.ReadAsStringAsync();
        }

        private async void AppendClippingsToPage(string pageId, List<Clipping> clippings)
        {
            string requestBody = NoteRequest.UpdatePage(clippings);
            var jsonObject = new JsonObject();
            jsonObject.Add("target", JsonValue.CreateStringValue(String.Concat(@"#", NoteRequest.dataId)));
            jsonObject.Add("action", JsonValue.CreateStringValue("append"));
            jsonObject.Add("position", JsonValue.CreateStringValue("after"));
            jsonObject.Add("content", JsonValue.CreateStringValue(requestBody));
            var jsonArray = new JsonArray();
            jsonArray.Add(jsonObject);

            var method = new HttpMethod("PATCH");
            var reqString = jsonArray.ToString();
            var appendApi = new Uri(baseUri, String.Format("pages/{0}/content", pageId));
            var request = new HttpRequestMessage(method, appendApi);
            string token = await Account.GetTokenSilentlyAsync();
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);
            request.Content = new HttpStringContent(reqString, UnicodeEncoding.Utf8, @"application/json");
            HttpResponseMessage httpResponse = await client.SendRequestAsync(request);
            HttpStatusCode code = httpResponse.StatusCode;
            string resp = await httpResponse.Content.ReadAsStringAsync();
        }
    }
}
