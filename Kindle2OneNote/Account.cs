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
        private static readonly string storedAccountIdKey = "CurrentUserId";
        private static readonly string storedProviderIdKey = "CurrentUserProviderId";
        private static readonly string storedProviderAuthorityKey = "CurrentUserProviderAuthority";
        private static readonly string scope = @"office.onenote, office.onenote_update_by_app";
        private static readonly string providerId = @"https://login.microsoft.com";
        private static readonly string providerAuthority = @"consumers";
        
        public static void SignIn()
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildPaneAsync;
            AccountsSettingsPane.Show();
        }

        public static async Task SignOut()
        {
            if (!IsSignedIn())
                return;

            WebAccount account = await GetWebAccount();
            if (account != null)
            {
                await account.SignOutAsync();
                account = null;
            }
            RemoveAccountData();
        }

        public static bool IsSignedIn()
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(storedAccountIdKey) &&
                ApplicationData.Current.LocalSettings.Values.ContainsKey(storedProviderIdKey) &&
                ApplicationData.Current.LocalSettings.Values.ContainsKey(storedProviderAuthorityKey);
        }

        public static async Task<string> GetToken()
        {
            WebAccount account = await GetWebAccount();
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);

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
        
        private static async Task<WebAccount> GetWebAccount()
        {
            String accountID = ApplicationData.Current.LocalSettings.Values[storedAccountIdKey] as String;
            String providerID = ApplicationData.Current.LocalSettings.Values[storedProviderIdKey] as String;
            
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountID);
            if (account == null)
            {
                RemoveAccountData();
            }

            return account;
        }

        private static void StoreWebAccount(WebAccount account)
        {
            ApplicationData.Current.LocalSettings.Values[storedAccountIdKey] = account.Id;
            ApplicationData.Current.LocalSettings.Values[storedProviderIdKey] = account.WebAccountProvider.Id;
            ApplicationData.Current.LocalSettings.Values[storedProviderAuthorityKey] = account.WebAccountProvider.Authority;
        }

        private static void RemoveAccountData()
        {
            ApplicationData.Current.LocalSettings.Values.Remove(storedAccountIdKey);
            ApplicationData.Current.LocalSettings.Values.Remove(storedProviderIdKey);
            ApplicationData.Current.LocalSettings.Values.Remove(storedProviderAuthorityKey);
        }

        private static async void BuildPaneAsync(AccountsSettingsPane s, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId, providerAuthority);
            var command = new WebAccountProviderCommand(msaProvider, GetMsaTokenAsync);
            e.WebAccountProviderCommands.Add(command);

            deferral.Complete();
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= BuildPaneAsync;
        }

        private static async void GetMsaTokenAsync(WebAccountProviderCommand command)
        {
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, scope);
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);
            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                WebAccount account = result.ResponseData[0].WebAccount;
                StoreWebAccount(account);
            }
            Presenter.Instance.OnSignInComplete();
        }
    }
}
