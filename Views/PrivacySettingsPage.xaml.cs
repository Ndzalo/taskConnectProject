
using Microsoft.Maui.Controls;
using taskConnectProject.ViewModel;

namespace taskConnectProject.Views;

public partial class PrivacySettingsPage : ContentPage
{
	public PrivacySettingsPage()
	{
		InitializeComponent();
        BindingContext = new PrivacySettingsViewModel();

    }
}