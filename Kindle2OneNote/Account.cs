using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;
using Windows.Security.Authentication.Web.Core;

namespace Kindle2OneNote
{
    static class Account
    {
        public static string UserName { get; set; }
        public static string Picture { get; set; }

        private static WebAccount account;
        private static WebAccountProvider provider;

        private static readonly string scope = @"office.onenote, office.onenote_update_by_app";

        public static void SignIn()
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildPaneAsync;
            AccountsSettingsPane.Show();
        }

        public static bool IsSignedIn()
        {
            string userId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();
            return userId != null;
        }

        private static async Task LoadAccount()
        {
            string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
            string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

            if (null == providerId || null == accountId)
            {
                return;
            }

            provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);
        }

        private static async void BuildPaneAsync(AccountsSettingsPane s, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");
            var command = new WebAccountProviderCommand(msaProvider, GetMsaTokenAsync);
            e.WebAccountProviderCommands.Add(command);

            deferral.Complete();
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= BuildPaneAsync;
        }

        private static async void GetMsaTokenAsync(WebAccountProviderCommand command)
        {
            var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
            var page = (MainPage)frame.Content;
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, scope);
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);
            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                account = result.ResponseData[0].WebAccount;
                StoreWebAccount();
                page.OnSignInStatus(true);
            }
            else
            {
                page.OnSignInStatus(false);
            }
        }

        private static void StoreWebAccount()
        {
            ApplicationData.Current.LocalSettings.Values["CurrentUserId"] = account.Id;
            ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"] = account.WebAccountProvider.Id;
        }

        public static async Task<string> GetTokenSilentlyAsync()
        {
            await LoadAccount();
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

        public static async Task SignOut()
        {
            if (!IsSignedIn())
                return;

            await account.SignOutAsync();
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserProviderId");
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserId");

            var frame = (Windows.UI.Xaml.Controls.Frame)Windows.UI.Xaml.Window.Current.Content;
            var page = (MainPage)frame.Content;
            page.OnSignInStatus(false);
        }
    }
}
