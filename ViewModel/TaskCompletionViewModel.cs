using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Windows.Input;
using taskConnectProject.Models;
using taskConnectProject.Views;
using Payment = taskConnectProject.Views.Payment;

namespace taskConnectProject.ViewModel
{
    public class TaskCompletionViewModel : BaseViewModel
    {
        private readonly FirebaseClient _firebaseClient;
        private string _taskId;
        private PostedTask _task;

        // Add proper properties with notification
        private string _taskName;
        public string TaskName
        {
            get => _taskName;
            set => SetProperty(ref _taskName, value);
        }

        private string _taskLocation;
        public string TaskLocation
        {
            get => _taskLocation;
            set => SetProperty(ref _taskLocation, value);
        }

        private decimal _taskPrice;
        public decimal TaskPrice
        {
            get => _taskPrice;
            set => SetProperty(ref _taskPrice, value);
        }

        public ICommand TaskerConfirmCommand { get; }
        public ICommand TaskeeConfirmCommand { get; }
        public ICommand ProceedToPaymentCommand { get; }
        public ICommand NavigateToRatingCommand { get; }

        public TaskCompletionViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            TaskerConfirmCommand = new Command(async () => await TaskerConfirm(), () => _task != null);
            TaskeeConfirmCommand = new Command(async () => await TaskeeConfirm(), () => _task != null);
            ProceedToPaymentCommand = new Command(async () => await ProceedToPayment(), () => _task != null);
            NavigateToRatingCommand = new Command(async () => await NavigateToRating(), () => _task != null);
        }

        public async Task Initialize(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                await Shell.Current.DisplayAlert("Error", "Invalid task ID", "OK");
                return;
            }

            _taskId = taskId;
            await LoadTask();
            // Notify commands to update their CanExecute status
            ((Command)TaskerConfirmCommand).ChangeCanExecute();
            ((Command)TaskeeConfirmCommand).ChangeCanExecute();
            ((Command)ProceedToPaymentCommand).ChangeCanExecute();
            ((Command)NavigateToRatingCommand).ChangeCanExecute();
        }

        private async Task LoadTask()
        {
            try
            {
                _task = await _firebaseClient
                    .Child("tasks")
                    .Child(_taskId)
                    .OnceSingleAsync<PostedTask>();

                if (_task != null)
                {
                    TaskName = _task.Task;
                    TaskLocation = _task.Location;
                    TaskPrice = _task.Price;

                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Task not found", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading task: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load task details.", "OK");
            }
        }

        private async Task TaskerConfirm()
        {
            if (_task == null) return;

            try
            {
                var updatedTask = await _firebaseClient
                    .Child("tasks")
                    .Child(_taskId)
                    .OnceSingleAsync<PostedTask>();

                if (updatedTask == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Task not found. Please try again.", "OK");
                    return;
                }

                updatedTask.TaskerConfirmed = true;

                // Check if both tasker and taskee confirmed
                if (updatedTask.TaskerConfirmed && updatedTask.TaskeeConfirmed)
                {
                    updatedTask.Status = "Completed";  // Change the task status to "Completed"
                }

                await _firebaseClient
                    .Child("tasks")
                    .Child(_taskId)
                    .PutAsync(updatedTask);

                // Delay to allow Firebase to process the update
                await Task.Delay(500);

                _task.TaskerConfirmed = true;
                OnPropertyChanged(nameof(_task.TaskerConfirmed));

                await Shell.Current.DisplayAlert("Success", "Tasker confirmation recorded.", "OK");
                await Shell.Current.GoToAsync(nameof(Payment));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming task: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to confirm task: {ex.Message}", "OK");
            }
        }


        private async Task TaskeeConfirm()
        {
            if (_task == null) return;

            try
            {
                // Create a new instance to ensure we're not modifying cached data
                var updatedTask = await _firebaseClient
                    .Child("tasks")
                    .Child(_taskId)
                    .OnceSingleAsync<PostedTask>();

                if (updatedTask != null)
                {
                    updatedTask.TaskeeConfirmed = true;

                    // Check if both tasker and taskee confirmed
                    if (updatedTask.TaskerConfirmed && updatedTask.TaskeeConfirmed)
                    {
                        updatedTask.Status = "Completed";  // Change the task status to "Completed"
                    }

                    await _firebaseClient
                        .Child("tasks")
                        .Child(_taskId)
                        .PutAsync(updatedTask);

                    _task.TaskeeConfirmed = true;
                    OnPropertyChanged(nameof(TaskeeConfirmCommand));
                    await Shell.Current.DisplayAlert("Success", "Taskee confirmation recorded.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming task: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to confirm task.", "OK");
            }
        }


        private async Task ProceedToPayment()
        {
            if (_task == null) return;

            if (_task.TaskerConfirmed && _task.TaskeeConfirmed)
            {
                // Remove the completed task from UI
                MessagingCenter.Send(this, "TaskCompleted", _taskId);

                // Navigate to Payment Page
                var navigationParameter = new Dictionary<string, object>
        {
            { "TaskId", _taskId }
        };
                await Shell.Current.GoToAsync("Payment", navigationParameter);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Both tasker and taskee must confirm the task completion.", "OK");
            }
        }


        private async Task NavigateToRating()
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "TaskId", _taskId },
                { "ToUserEmail", _task.TaskerEmail } 
            };
            await Shell.Current.GoToAsync($"{nameof(RatingPage)}", navigationParameter);
        }

    }
}