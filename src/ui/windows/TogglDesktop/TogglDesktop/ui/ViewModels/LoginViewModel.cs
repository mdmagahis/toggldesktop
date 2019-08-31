using System.Collections.Generic;
using System.Linq;

namespace TogglDesktop.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private IList<CountryViewModel> countries;
        private CountryViewModel selectedCountry;

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

        private void OnDisplayCountries(List<Toggl.TogglCountryView> list)
        {
            Countries = list.Select(c => new CountryViewModel(c)).ToArray();
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
    }
}