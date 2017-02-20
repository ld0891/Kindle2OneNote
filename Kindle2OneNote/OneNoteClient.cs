using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.Web.Http.Headers;
using Windows.System;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;
using Windows.Data.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Security.Credentials;
using Windows.Storage;


namespace Kindle2OneNote
{
    public struct Account
    {
        public string UserName { get; set; }
        public string Picture { get; set; }
    }

    public sealed class OneNoteClient
    {
        private static volatile OneNoteClient instance = null;
        private static object syncRoot = new Object();
        private static WebAccount account;

        private static readonly int notFound = -1;
        private static readonly string scope = "office.onenote";
        private static readonly string baseUrl = @"https://www.onenote.com/api/v1.0/me/notes/";

        private OneNoteClient() { }

        public static OneNoteClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new OneNoteClient();
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
            string rawResponse = await QuerySections();
            List<Section> sections = ParseResponse(rawResponse);
            List<Notebook> notebooks = BuildNotebooksFromSections(sections);
        }

        private async Task<string> QuerySections()
        {
            string token = await GetTokenSilentlyAsync();
            var baseUri = new Uri(baseUrl);
            var queryApi = new Uri(baseUri, @"sections");
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var infoResult = await client.GetAsync(queryApi);
                string content = await infoResult.Content.ReadAsStringAsync();
                return content;
            }
        }

        private List<Section> ParseResponse(string response)
        {
            var section = new Section();
            var sections = new List<Section>();
            var jsonObject = JsonObject.Parse(response);

            foreach (IJsonValue jsonValue in jsonObject.GetNamedArray("value", new JsonArray()))
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
    }
}
