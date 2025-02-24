using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using taskConnectProject.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Firebase.Database.Query;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.Input;

namespace taskConnectProject.ViewModel
{
    public class ProfilePageViewModel : INotifyPropertyChanged
    {
        private readonly FirebaseClient _firebaseClient;
        private string _userName;
        private string _email;
        private string _phoneNumber;
        private string _idNumber;
        private bool _isProfileDetailsVisible;

        public bool IsProfileDetailsVisible
        {
            get => _isProfileDetailsVisible;
            set => SetProperty(ref _isProfileDetailsVisible, value);
        }

        public string Name
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
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

        public ICommand LogoutCommand { get; }
        public ICommand ToggleProfileDetailsCommand { get; }
        public ICommand NavigateToHelpCommand { get; }

        public ProfilePageViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            LogoutCommand = new Command(async () => await LogoutAsync());
            ToggleProfileDetailsCommand = new Command(() => IsProfileDetailsVisible = !IsProfileDetailsVisible);

            NavigateToHelpCommand = new AsyncRelayCommand(NavigateToHelp);
            // Initialize with details visible
            IsProfileDetailsVisible = true;
        }

        public async Task LoadUserProfile()
        {
            try
            {
                var userEmail = Preferences.Get("UserEmail", string.Empty);
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var users = await _firebaseClient
                        .Child("users")
                        .OnceAsync<User>();

                    // Use case-insensitive comparison for email
                    var user = users.FirstOrDefault(u =>
                        string.Equals(u.Object.Email, userEmail, StringComparison.OrdinalIgnoreCase))?.Object;

                    if (user != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Name = user.Name ?? "Not Available";
                            Email = user.Email ?? "Not Available";
                            phoneNumber = user.phoneNumber ?? "Not Available";
                            idNumber = user.idNumber ?? "Not Available";
                        });
                    }
                    else
                    {
                        Console.WriteLine($"User not found for email: {userEmail}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profile: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load profile. Please try again.", "OK");
            }
        }

        private async Task NavigateToHelp()
        {
            await Shell.Current.GoToAsync("HelpPage");
        }

        private async Task LogoutAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");
            if (confirm)
            {
                Preferences.Remove("UserEmail");
                await Shell.Current.GoToAsync("/LoginPage");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
