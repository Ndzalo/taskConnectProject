using taskConnectProject.ViewModel;
namespace taskConnectProject.Views;

public partial class TaskeeCompletionPage : ContentPage, IQueryAttributable  // Add IQueryAttributable here
{
    private TaskCompletionViewModel _viewModel;

    public TaskeeCompletionPage()
    {
        InitializeComponent();
        _viewModel = new TaskCompletionViewModel();
        BindingContext = _viewModel;
    }

    // Remove OnNavigatedTo and replace with ApplyQueryAttributes
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("TaskId", out var taskId))
        {
            await _viewModel.Initialize(taskId.ToString());
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "No task ID provided", "OK");
            
        }
    }
}