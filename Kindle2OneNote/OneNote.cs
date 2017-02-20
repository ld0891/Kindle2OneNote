using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.Web.Http.Headers;
using Windows.System;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Security.Credentials;
using Windows.Storage;

using System.Text;


namespace Kindle2OneNote
{
    public struct Account
    {
        public string UserName { get; set; }
        public string Picture { get; set; }
    }

    public sealed class OneNote
    {
        private static volatile OneNote instance = null;
        private static object syncRoot = new Object();
        private static WebAccount account;
        private static HttpClient client;

        private static readonly int notFound = -1;
        private static readonly string valueKey = @"value";
        private static readonly string scope = @"office.onenote, office.onenote_update_by_app";
        private static readonly string baseUrl = @"https://www.onenote.com/api/v1.0/me/notes/";
        private static readonly string timeFormat = @"yyyy/MM/ddTHH:mm:sszzz";
        private static readonly string contentType = @"application/xhtml+xml";
        private static readonly string pageHtmlFormat = @"<!DOCTYPE html>
                                                          <html>
                                                              <head>
                                                                  <title>{0}</title>
                                                                  <meta name=""created"" content=""{1}"" />
                                                              </head>
                                                          </html>";

        private OneNote() { }

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
                            client = new HttpClient();
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                        }
                    }
                }

                return instance;
            }
        }

        public void SignIn()
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildPaneAsync;
            AccountsSettingsPane.Show();
        }

        private async void BuildPaneAsync(AccountsSettingsPane s, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", "consumers");
            var command = new WebAccountProviderCommand(msaProvider, GetMsaTokenAsync);
            e.WebAccountProviderCommands.Add(command);

            deferral.Complete();
        }

        private async void GetMsaTokenAsync(WebAccountProviderCommand command)
        {
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, scope);
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);
            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                string token = result.ResponseData[0].Token;
                account = result.ResponseData[0].WebAccount;
                StoreWebAccount();
            }
        }

        private void StoreWebAccount()
        {
            ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"] = account.WebAccountProvider.Id;
            ApplicationData.Current.LocalSettings.Values["CurrentUserId"] = account.Id;
        }


        private async Task<string> GetTokenSilentlyAsync()
        {
            string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
            string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

            if (null == providerId || null == accountId)
            {
                return null;
            }

            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

            WebTokenRequest request = new WebTokenRequest(provider, scope);

            WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);
            if (result.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
            {
                // Unable to get a token silently - you'll need to show the UI
                return null;
            }
            else if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                // Success
                return result.ResponseData[0].Token;
            }
            else
            {
                // Other error 
                return null;
            }
        }

        public async void SignOut()
        {
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserProviderId");
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserId");
            await account.SignOutAsync();
        }

        public async Task<Account> GetAccount()
        {
            return new Account();
        }

        public async void GetNotebooks()
        {
            /*
             * section: 0-3A5991079B7F1889!21146
             * page: 0-31d3191622644177b0f9164dabcb16f7!72-3A5991079B7F1889!21146
            string rawResponse = await QuerySections();
            List<Section> sections = ParseResponse(rawResponse);
            List<Notebook> notebooks = BuildNotebooksFromSections(sections);
            */

        }

        private async Task<string> QuerySections()
        {
            var queryApi = new Uri(new Uri(baseUrl), @"sections");
            string token = await GetTokenSilentlyAsync();
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            var infoResult = await client.GetAsync(queryApi);
            string content = await infoResult.Content.ReadAsStringAsync();
            return content;
        }

        private List<Section> ParseSectionResponse(string response)
        {
            var section = new Section();
            var sections = new List<Section>();
            var jsonObject = JsonObject.Parse(response);

            foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(valueKey, new JsonArray()))
            {
                if (jsonValue.ValueType == JsonValueType.Object)
                {
                    section = new Section(jsonValue.GetObject().ToString());
                    sections.Add(section);
                }
            }
            return sections;
        }

        private List<Notebook> BuildNotebooksFromSections(List<Section> sections)
        {
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

        private async void CreatePageInSection(string sectionId, string pageName)
        {
            string timeString = DateTime.Now.ToString(timeFormat);
            string token = await GetTokenSilentlyAsync();
            var createApi = new Uri(new Uri(baseUrl), String.Format("sections/{0}/pages", sectionId));

            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);
            HttpStringContent content = new HttpStringContent(String.Format(pageHtmlFormat, pageName, timeString),
                Windows.Storage.Streams.UnicodeEncoding.Utf8,
                contentType);
            HttpResponseMessage httpResponse = await client.PostAsync(createApi, content);
            HttpStatusCode code = httpResponse.StatusCode;
        }

        private async void QueryPagesInSection(string sectionId)
        {
            string token = await GetTokenSilentlyAsync();
            var queryApi = new Uri(new Uri(baseUrl), String.Format("sections/{0}/pages", sectionId));
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            var infoResult = await client.GetAsync(queryApi);
            string content = await infoResult.Content.ReadAsStringAsync();
            ParseNotePageResponse(content);
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
        
        private async void AppendClippingsToPage(string pageId, List<Clipping> clippings)
        {
            string token = await GetTokenSilentlyAsync();
            var appendApi = new Uri(new Uri(baseUrl), String.Format("pages/{0}/content", pageId));
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            HttpStringContent content = new HttpStringContent("",
                Windows.Storage.Streams.UnicodeEncoding.Utf8,
                contentType);
            HttpResponseMessage httpResponse = await client.PostAsync(appendApi, content);
            HttpStatusCode code = httpResponse.StatusCode;
        }
    }
}
