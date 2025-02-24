using taskConnectProject.ViewModel;
namespace taskConnectProject.Views;

public partial class TasksMenu : ContentPage
{
   

    public TasksMenu()
	{
		InitializeComponent();
		BindingContext = new HomePageViewModel();
		//BindingContext = new TaskBookingViewModel();

    }
}