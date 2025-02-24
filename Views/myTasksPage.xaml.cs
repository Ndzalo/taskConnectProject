using taskConnectProject.ViewModel;

namespace taskConnectProject.Views;

public partial class myTasksPage : ContentPage
{
	public myTasksPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TaskViewModel viewModel)
        {
            viewModel.LoadTasksCommand.Execute(null);
        }
    }

}