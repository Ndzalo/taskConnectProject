using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;


namespace taskConnectProject.ViewModel
{
    public class WelcomePageViewModel : BindableObject
    {
        public ICommand NavigateToLoginCommand { get; }

        public WelcomePageViewModel()
        {
            NavigateToLoginCommand = new Command(async () => await Shell.Current.GoToAsync("LoginPage"));
        }

        private async void NavigateToLogin()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

}
