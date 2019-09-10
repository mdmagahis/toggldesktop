using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TogglDesktop.Diagnostics;
using System.Windows.Navigation;
using TogglDesktop.ViewModels;

namespace TogglDesktop
{
    public partial class LoginView : IMainView
    {
        private enum ConfirmAction
        {
            Unknown = 0,
            LogIn,
            SignUp
        }

        private readonly Storyboard confirmSpinnerAnimation;

        private Action onLogin;
        private object opacityAnimationToken;

        private ConfirmAction confirmAction = ConfirmAction.Unknown;
        private bool loggingIn;
        private bool countriesLoaded = false;
        private LoginViewModel viewModel;

        public LoginView()
        {
            this.InitializeComponent();
            ViewModel = new LoginViewModel();
            this.confirmSpinnerAnimation = (Storyboard)this.Resources["RotateConfirmSpinner"];

            this.IsVisibleChanged += this.onIsVisibleChanged;
        }

        public LoginViewModel ViewModel
        {
            get => viewModel;
            set => DataContext = viewModel = value;
        }

        private void onIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                this.setConfirmAction(ConfirmAction.LogIn);
        }

        #region events

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.tryConfirm();
                e.Handled = true;
            }
        }

        private void onConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            this.tryConfirm();
        }

        private void onSignupLoginToggleClick(object sender, RoutedEventArgs e)
        {
            switch (this.confirmAction)
            {
                case ConfirmAction.LogIn:
                    this.setConfirmAction(ConfirmAction.SignUp);
                    break;
                case ConfirmAction.SignUp:
                    this.setConfirmAction(ConfirmAction.LogIn);
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid action '{0}' in login form.", this.confirmAction));
            }
        }

        private void onGoogleLoginClick(object sender, RoutedEventArgs e)
        {
            if (!validateGoogleLoginSignup())
            {
                return;
            }

            switch (this.confirmAction)
            {
                case ConfirmAction.LogIn:
                    ViewModel.GoogleLogin();
                    break;
                case ConfirmAction.SignUp:
                    ViewModel.GoogleSignup();
                    break;
                default:
                    throw new ArgumentException($"Invalid action '{this.confirmAction}' in login form.");
            }
        }

        private void onForgotPasswordButtonClick(object sender, RoutedEventArgs e)
        {
            Toggl.PasswordForgot();
        }

        #endregion

        #region controlling

        private void setConfirmAction(ConfirmAction action)
        {
            switch (action)
            {
                case ConfirmAction.LogIn:
                    this.confirmButtonText.Text = "LOG IN";
                    this.forgotPasswordButton.Visibility = Visibility.Visible;
                    this.googleLoginButtonTextBlock.Text = "LOG IN WITH GOOGLE";
                    this.countryLabel.Visibility = Visibility.Collapsed;
                    this.countrySelect.Visibility = Visibility.Collapsed;
                    this.tosCheckbox.Visibility = Visibility.Collapsed;
                    this.signupLoginToggle.Content = "Sign up for free";
                    break;
                case ConfirmAction.SignUp:
                    this.confirmButtonText.Text = "SIGN UP";
                    this.forgotPasswordButton.Visibility = Visibility.Collapsed;
                    this.googleLoginButtonTextBlock.Text = "SIGN UP WITH GOOGLE";
                    this.countryLabel.Visibility = Visibility.Visible;
                    this.countrySelect.Visibility = Visibility.Visible;
                    this.tosCheckbox.Visibility = Visibility.Visible;
                    this.signupLoginToggle.Content = "Log in";
                    Task.Factory.StartNew(() =>
                    {
                        getCountries();
                    });
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid action '{0}' in login form.", action));
            }
            this.confirmAction = action;
        }

        private void getCountries()
        {
            Dispatcher.Invoke(() =>
            {
                if (!this.countriesLoaded)
                {
                    Toggl.GetCountries();
                    this.countriesLoaded = true;
                }
            });            
        }

        private void tryConfirm()
        {
            if (!this.validateLoginSignup())
            {
                return;
            }
            switch (this.confirmAction)
            {
                case ConfirmAction.LogIn:
                    this.confirm(Toggl.Login, "log in");
                    break;
                case ConfirmAction.SignUp:
                    this.confirm(Toggl.Signup, "sign up");
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid action '{0}' in login form.", this.confirmAction));
            }
        }

        private async void confirm(Func<string, string, long, bool> confirmAction, string actionName)
        {
            using (Performance.Measure("attempting " + actionName))
            {
                var success = false;
                try
                {
                    this.loggingIn = true;
                    this.disableForm();

                    var email = this.emailTextBox.Text;
                    var password = this.passwordBox.Text;

                    success = await Task.Run(() => confirmAction(email, password, ViewModel.SelectedCountry?.ID ?? -1));
                }
                finally
                {
                    this.loggingIn = false;
                    if (success && this.onLogin != null)
                    {
                        var action = this.onLogin;
                        this.onLogin = null;
                        action();
                    }
                    else
                    {
                        this.reset();
                    }
                }
            }
        }

        private void enableForm()
        {
            this.formPanel.IsEnabled = true;
            this.formPanel.Opacity = 1;
            this.confirmButtonText.Visibility = Visibility.Visible;
            this.confirmButtonSpinner.Visibility = Visibility.Collapsed;
            this.confirmSpinnerAnimation.Stop();
        }

        private void disableForm()
        {
            this.formPanel.IsEnabled = false;
            this.formPanel.Opacity = 0.5;
            this.confirmButtonText.Visibility = Visibility.Collapsed;
            this.confirmButtonSpinner.Visibility = Visibility.Visible;
            this.confirmSpinnerAnimation.Begin();
        }

        private bool validateLoginSignup()
        {
            if (this.emailTextBox.Text == "")
            {
                this.emailTextBox.Focus();
                Toggl.NewError("Please enter valid email address", true);
                return false;
            }
            if (this.passwordBox.Text == "")
            {
                this.passwordBox.Focus();
                Toggl.NewError("A password is required", true);
                return false;
            }

            if (this.confirmAction == ConfirmAction.SignUp
                && !validateMandatorySignupFields())
            {
                return false;
            }

            return true;
        }

        private bool validateMandatorySignupFields()
        {
            if (this.ViewModel.SelectedCountry != null)
            {
                this.countrySelect.Focus();
                Toggl.NewError("Please select Country before signing up", true);
                return false;
            }
            if (this.tosCheckbox.IsChecked != true)
            {
                this.tosCheckbox.Focus();
                Toggl.NewError("You must agree to the terms of service and privacy policy to use Toggl", true);

                return false;
            }

            return true;
        }

        private bool validateGoogleLoginSignup()
        {
            if (this.confirmAction == ConfirmAction.SignUp
                && !validateMandatorySignupFields())
            {
                return false;
            }

            return true;
        }

        private void reset()
        {
            this.enableForm();
            this.passwordBox.Clear();
        }

        #endregion


        #region fade in/out

        private const double opacityFadeTime = 0.25;

        public void Activate(bool allowAnimation)
        {
            this.opacityAnimationToken = null;

            if (allowAnimation)
            {
                var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(opacityFadeTime));
                this.BeginAnimation(OpacityProperty, anim);
            }
            else
            {
                this.BeginAnimation(OpacityProperty, null);
            }

            this.reset();
            this.IsEnabled = true;
            this.Visibility = Visibility.Visible;
        }

        public void Deactivate(bool allowAnimation)
        {
            this.opacityAnimationToken = null;

            if (allowAnimation)
            {
                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(opacityFadeTime));
                this.opacityAnimationToken = anim;
                anim.Completed += (sender, args) =>
                {
                    if (this.opacityAnimationToken == anim)
                    {
                        this.Visibility = Visibility.Collapsed;
                        this.reset();
                    }
                };
                if (this.loggingIn)
                {
                    this.onLogin = () => this.BeginAnimation(OpacityProperty, anim);
                }
                else
                {
                    this.BeginAnimation(OpacityProperty, anim);
                }
            }
            else
            {
                this.BeginAnimation(OpacityProperty, null);
                this.Visibility = Visibility.Collapsed;
            }

            this.IsEnabled = false;
        }


        #endregion

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Toggl.OpenInBrowser(e.Uri.ToString());
        }
    }
}
