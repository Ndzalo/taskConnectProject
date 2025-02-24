using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using taskConnectProject.Views;

namespace taskConnectProject.ViewModel
{
    public class TaskApplicantsViewModel : BaseViewModel
    {

        private readonly FirebaseClient _firebaseClient;
        private ObservableCollection<Applicant> _applicants;
        public ObservableCollection<Applicant> Applicants
        {
            get => _applicants;
            set => SetProperty(ref _applicants, value);
        }
        private string _taskKey;
        public string TaskKey
        {
            get => _taskKey;
            set
            {
                Console.WriteLine($"TaskKey property being set to: {value}");
                if (SetProperty(ref _taskKey, value))
                {
                    LoadApplicants();
                }
            }
        }
        public ICommand SelectApplicantCommand { get; }
        public ICommand ViewAcceptedOffersCommand { get; }

        public TaskApplicantsViewModel()
        {
            Applicants = new ObservableCollection<Applicant>();
            SelectApplicantCommand = new Command<Applicant>(async (applicant) => await SelectApplicant(applicant));
            ViewAcceptedOffersCommand = new Command(async () => await ViewAcceptedOffer());
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
        }

        // Method to initialize the ViewModel with the task identifier
        public async Task Initialize(string taskKey)
        {
            if (string.IsNullOrEmpty(taskKey))
            {
                await Shell.Current.DisplayAlert("Error", "Invalid task key", "OK");
                return;
            }

            TaskKey = taskKey;
            LoadApplicants(); // Make this method async
        }

        private async Task ViewAcceptedOffer()
        {
            if (string.IsNullOrEmpty(TaskKey)) return;

            try
            {
                // Fetch the task from Firebase
                var task = await _firebaseClient
                    .Child("tasks")
                    .Child(TaskKey)
                    .OnceSingleAsync<PostedTask>();

                if (task != null && !string.IsNullOrEmpty(task.SelectedApplicant))
                {
                    // Fetch the notification for the selected applicant
                    var notifications = await _firebaseClient
                        .Child("notifications")
                        .OnceAsync<TaskNotification>();

                    var acceptedNotification = notifications
                        .FirstOrDefault(n => n.Object.TaskId == TaskKey &&
                                             n.Object.ApplicantEmail == task.SelectedApplicant &&
                                             n.Object.Status == "Accepted");

                    if (acceptedNotification != null)
                    {
                        // Show the accepted offer details
                        await Shell.Current.DisplayAlert(
                            "Accepted Offer",
                            $"{task.SelectedApplicant} has accepted your offer for the task: {task.Task}.",
                            "OK"
                        );
                        await Shell.Current.GoToAsync($"{nameof(TaskCompletionPage)}?TaskId={TaskKey}");

                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Info", "No accepted offers found for this task.", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No applicant selected or task not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing accepted offers: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to view accepted offers. Please try again.", "OK");
                Console.WriteLine($"Error navigating to TaskCompletionPage: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to navigate. Please try again.", "OK");
            }
        }

        private async Task ProceedToConfirm(PostedTask task)
        {


        }

        private async void LoadApplicants()
        {
            if (string.IsNullOrEmpty(TaskKey))
            {
                Console.WriteLine("TaskKey is null or empty in LoadApplicants");
                return;
            }

            try
            {
                {
                    Console.WriteLine($"Starting to load applicants for TaskKey: {TaskKey}");

                    var tasksQuery = await _firebaseClient
                        .Child("tasks")
                        .OnceAsync<PostedTask>();

                    Console.WriteLine($"Retrieved {tasksQuery.Count()} tasks from Firebase");

                    var taskData = tasksQuery.FirstOrDefault(t => t.Key == TaskKey);

                    if (taskData != null)
                    {
                        Console.WriteLine($"Found matching task with key: {taskData.Key}");
                        var task = taskData.Object;

                        if (task.Applicants != null && task.Applicants.Any())
                        {
                            Applicants = new ObservableCollection<Applicant>(
                                task.Applicants.Select(email => new Applicant { Email = email })
                            );
                            Console.WriteLine($"Loaded {Applicants.Count} applicants");
                        }
                        else
                        {
                            Console.WriteLine("No applicants found for this task");
                            Applicants.Clear();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No task found with key: {TaskKey}");
                        await Shell.Current.DisplayAlert("Error", "Task not found", "OK");
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadApplicants: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", "Failed to load applicants. Please try again.", "OK");
            }
        }


        private async Task SelectApplicant(Applicant applicant)
        {
            if (applicant == null || string.IsNullOrEmpty(TaskKey)) return;

            try
            {
                // Fetch the task from Firebase
                var task = await _firebaseClient
                    .Child("tasks")
                    .Child(TaskKey)
                    .OnceSingleAsync<PostedTask>();

                if (task != null)
                {
                    // Update task properties
                    task.SelectedApplicant = applicant.Email;
                    task.IsBooked = true;
                    task.HasNewApplicant = false;
                    task.Status = "Assigned"; // Add a status field

                    // Create notification for the selected applicant
                    var notification = new TaskNotification
                    {
                        TaskId = TaskKey,
                        TaskName = task.Task,
                        TaskLocation = task.Location,
                        TaskPrice = task.Price,
                        ApplicantEmail = applicant.Email,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        Message = $"You have been selected for the task: {task.Task}",
                        IsRead = false
                    };

                    // Save notification to Firebase
                    await _firebaseClient
                        .Child("notifications")
                        .PostAsync(notification);

                    // Update task in Firebase
                    await _firebaseClient
                        .Child("tasks")
                        .Child(TaskKey)
                        .PutAsync(task);

                    // Show success message to tasker
                    await Shell.Current.DisplayAlert(
                        "Success",
                        $"Selected {applicant.Email} for the task. They will be notified.",
                        "OK"
                    );

                    // Navigate back
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting applicant: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to select applicant. Please try again.", "OK");
            }
        }

        public class Applicant
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Image { get; set; }
        }

        public class TaskNotification
        {
            public string TaskId { get; set; }
            public string TaskName { get; set; }
            public string TaskLocation { get; set; }
            public decimal TaskPrice { get; set; }
            public string ApplicantEmail { get; set; }
            public string TaskerEmail { get; set; }
            public string Status { get; set; } = "Pending";
            public DateTime CreatedAt { get; set; }
            public string Message { get; set; }
            public bool IsRead { get; set; }
        }
    }
}