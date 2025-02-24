using taskConnectProject.ViewModel;
using Microsoft.Maui.Controls;

namespace taskConnectProject.Views
{
    [QueryProperty(nameof(TaskKey), "TaskKey")]
    public partial class TaskApplicantsPage : ContentPage
    {
        private TaskApplicantsViewModel _viewModel;

        public string TaskKey
        {
            set
            {
                if (_viewModel != null)
                {
                    Console.WriteLine($"Setting TaskKey: {value}");
                    _viewModel.TaskKey = value;
                }
            }
        }

        public TaskApplicantsPage()
        {
            InitializeComponent();
            _viewModel = new TaskApplicantsViewModel();
            BindingContext = _viewModel;
        }

        /*protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("TaskApplicantsPage OnAppearing");
        }*/
    }
}