using taskConnectProject.ViewModel;

namespace taskConnectProject.Views;
public partial class TaskerMenu : ContentPage
{
    private TaskerPageViewModel _viewModel;

    public TaskerMenu()
    {
        InitializeComponent();
        _viewModel = new TaskerPageViewModel();
        BindingContext = _viewModel;
    }

   
    
}