using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Util;

namespace TogglDesktop.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private IList<CountryViewModel> countries;
        private CountryViewModel selectedCountry;
        private ConfirmAction confirmAction;

        public LoginViewModel()
        {
            Toggl.OnDisplayCountries += OnDisplayCountries;
        }

        public IList<CountryViewModel> Countries
        {
            get => countries;
            set => RaiseAndSetIfChanged(ref countries, value);
        }

        public CountryViewModel SelectedCountry
        {
            get => selectedCountry;
            set => RaiseAndSetIfChanged(ref selectedCountry, value);
        }

        public ConfirmAction SelectedConfirmAction
        {
            get => confirmAction;
            set => RaiseAndSetIfChanged(ref confirmAction, value);
        }

        private static async void GoogleAuth(Action<string> authAction, string authProcessName)
        {
            try
            {
                var credential = await obtainGoogleUserCredentialAsync();
                authAction(credential.Token.AccessToken);
                await credential.RevokeTokenAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("access_denied") ||
                    (ex.InnerException != null &&
                     ex.InnerException.Message.Contains("access_denied")))
                {
                    Toggl.NewError($"{authProcessName} process was canceled", true);
                }
                else
                {
                    Toggl.NewError(ex.Message, false);
                }
            }
        }

        public async void GoogleSignup()
        {
            GoogleAuth(accessToken => Toggl.GoogleSignup(accessToken, SelectedCountry?.ID ?? default), "Signup");
        }

        public static async void GoogleLogin()
        {
            GoogleAuth(accessToken => Toggl.GoogleLogin(accessToken), "Login");
        }

        private void OnDisplayCountries(List<Toggl.TogglCountryView> list)
        {
            Countries = list.Select(c => new CountryViewModel(c)).ToArray();
        }

        private static async Task<UserCredential> obtainGoogleUserCredentialAsync()
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "426090949585-uj7lka2mtanjgd7j9i6c4ik091rcv6n5.apps.googleusercontent.com",
                    ClientSecret = "6IHWKIfTAMF7cPJsBvoGxYui"
                },
                new[]
                {
                    Oauth2Service.Scope.UserinfoEmail,
                    Oauth2Service.Scope.UserinfoProfile
                },
                "user",
                CancellationToken.None);
            var isTokenExpired = credential.Token.IsExpired(SystemClock.Default);
            if (isTokenExpired)
            {
                await credential.RefreshTokenAsync(CancellationToken.None);
            }

            return credential;
        }

        public class CountryViewModel
        {
            private readonly Toggl.TogglCountryView countryView;
            public CountryViewModel(Toggl.TogglCountryView countryView)
            {
                this.countryView = countryView;
            }

            public string Name => countryView.Name;
            public long ID => countryView.ID;
        }

        public enum ConfirmAction
        {
            Unknown = 0,
            LogIn,
            SignUp
        }
    }
}