using taskConnectProject.ViewModel;
namespace taskConnectProject.Views;

public partial class TaskBookingPage : ContentPage
{
	public TaskBookingPage()
	{
		InitializeComponent();
	}
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (Shell.Current.CurrentItem.BindingContext is TaskBookingViewModel bookingViewModel)
        {
            if (Shell.Current.CurrentItem.BindingContext is IDictionary<string, object> navigationParams
                && navigationParams.ContainsKey("SelectedTask"))
            {
                var task = navigationParams["SelectedTask"] as AvailableTask;
                bookingViewModel.Initialize(task);
            }
        }
    }

}