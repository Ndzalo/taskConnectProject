using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Firebase.Database;
using Firebase.Database.Query;
using taskConnectProject.Services;
using taskConnectProject.Views;

namespace taskConnectProject.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly AuthService _authService;

        private string _email;
        private string _password;
        private string _statusMessage;

        public LoginViewModel()
        {
            _authService = new AuthService();
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(async () => await NavigateToRegisterPageAsync());
            ForgotPasswordCommand = new Command(async () => await NavigateToForgotPasswordPageAsync());
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter both email and password.";
                return;
            }

            try
            {
                // Authenticate the user using Firebase Authentication
                var authLink = await _authService.SignInAsync(Email, Password);

                if (authLink != null)
                {
                    // Store the user token and email after successful authentication
                    Preferences.Set("firebase_auth_token", authLink.FirebaseToken);
                    Preferences.Set("UserEmail", Email);

                    // Optionally, get user information from Firebase Realtime Database
                    var users = await _firebaseClient.Child("users").OnceAsync<Models.User>();
                    var user = users.FirstOrDefault(u => u.Object.Email == Email);

                    if (user != null)
                    {
                        Preferences.Set("UserName", user.Object.Name);
                    }

                    // Navigate to the tasks menu after successful login
                    await App.Current.MainPage.Navigation.PushAsync(new Views.TasksMenu());
                }
                else
                {
                    StatusMessage = "Invalid email or password.";
                }
            }
            catch (Firebase.Auth.FirebaseAuthException authEx)
            {
                string errorMessage = authEx.Message.ToLower();

                if (errorMessage.Contains("email"))
                    StatusMessage = "No account found with this email.";
                else if (errorMessage.Contains("password"))
                    StatusMessage = "Incorrect password. Please try again.";
                else if (errorMessage.Contains("too many attempts"))
                    StatusMessage = "Too many failed attempts. Please try again later.";
                else
                    StatusMessage = "Authentication failed. Please check your credentials.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }



        private async Task NavigateToRegisterPageAsync()
        {
            // Navigate to Register Page
            await App.Current.MainPage.Navigation.PushAsync(new Views.RegisterPage()); 
        }

        private async Task NavigateToForgotPasswordPageAsync()
        {
            // Navigate to Forgot Password Page
            await App.Current.MainPage.Navigation.PushAsync(new Views.ForgotPasswordPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
