using Microsoft.Maui.Controls;
using System;
using taskConnectProject.Views;

namespace taskConnectProject.Views
{
    public partial class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Navigate to Login Page
            //await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            // Navigate to Sign-Up Page
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
