using taskConnectProject.ViewModel;
namespace taskConnectProject.Views;

public partial class RatingPage : ContentPage
{
	public RatingPage()
	{
		InitializeComponent();
        BindingContext = new RatingViewModel();
    }
}