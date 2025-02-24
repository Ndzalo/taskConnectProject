using taskConnectProject.ViewModel;

namespace taskConnectProject.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfilePageViewModel _viewModel;

        public ProfilePage()
        {
            InitializeComponent();
            _viewModel = new ProfilePageViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadUserProfile();
        }

        private async void OnPrivacySettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PrivacySettingsPage());
        }

    }
}
