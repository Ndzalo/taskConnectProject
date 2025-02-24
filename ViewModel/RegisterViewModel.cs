using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
using System;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace taskConnectProject.ViewModel
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly FirebaseAuthProvider _authProvider;

        private string _name;
        private string _email;
        private string _password;
        private string _phoneNumber;
        private string _idNumber;
        private string _confirmPassword;
        private string _statusMessage;

        public RegisterViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCDUjGoGt8py3ok6DxPLlf1YtbrotXwZPU"));  // Firebase Authentication setup
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string phoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string idNumber
        {
            get => _idNumber;
            set => SetProperty(ref _idNumber, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RegisterCommand { get; }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                StatusMessage = "All fields are required!";
                return;
            }

            if (Password != ConfirmPassword)
            {
                StatusMessage = "Passwords do not match!";
                return;
            }

            try
            {
                // Create user in Firebase Authentication
                var authLink = await _authProvider.CreateUserWithEmailAndPasswordAsync(Email, Password);

                if (authLink != null)
                {
                    // Retrieve the user ID from Firebase Authentication
                    string userId = authLink.User.LocalId;

                    // Store user details in Firebase Realtime Database
                    var newUser = new Models.User
                    {
                        firebaseKey = userId,  // Store Firebase user ID
                        Name = Name,
                        Email = Email,
                        phoneNumber = phoneNumber,
                        idNumber = idNumber,
                        Timestamp = DateTime.UtcNow,
                        Password  = Password
                    };

                
                    await _firebaseClient
                        .Child("users")
                        .Child(userId)  // Save under unique user ID
                        .PutAsync(newUser);

                    StatusMessage = "User registered successfully!";
                    await Shell.Current.GoToAsync("LoginPage"); // Navigate to login page
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
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
