using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using taskConnectProject.Views;

namespace taskConnectProject.Views
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            string email = txtEmail?.Text;

            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Error", "Please enter your email address.", "OK");
                return;
            }

            // Simulate sending a password reset email
            await Task.Delay(1000); // Simulate network delay

            // Inform the user that the password reset link has been sent
            await DisplayAlert("Success", "Password reset link sent to your email.", "OK");

            // Optionally navigate back to the Login page
            await Navigation.PopAsync();
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
