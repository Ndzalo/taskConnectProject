using taskConnectProject.ViewModel;

namespace taskConnectProject.Views;

public partial class RatingPage1 : ContentPage
{
	public RatingPage1()
	{
		InitializeComponent();
        BindingContext = new RatingsPageViewModel();
    }
}