using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace taskConnectProject.ViewModel
{



    public class TaskerPageViewModel : BaseViewModel
    {
        private readonly FirebaseClient _firebaseClient;
        private ObservableCollection<TaskCategory> _taskCategories;
        public ObservableCollection<TaskCategory> TaskCategories
        {
            get => _taskCategories;
            set => SetProperty(ref _taskCategories, value);
        }

        private ObservableCollection<PostedTask> _postedTasks;
        public ObservableCollection<PostedTask> PostedTasks
        {
            get => _postedTasks;
            set => SetProperty(ref _postedTasks, value);
        }

        private decimal _totalExpenses;
        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }

        public ICommand SwitchToTaskeeCommand { get; }
        public ICommand SwitchToTaskerCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand CreateNewTaskCommand { get; }
        public ICommand ViewApplicantsCommand { get; }
        public ICommand CancelTaskCommand { get; }
       

        public TaskerPageViewModel()
        {

            // Initialize FirebaseClient

            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            // Initialize commands
            SwitchToTaskeeCommand = new Command(async () => await SwitchToTaskee());
            SwitchToTaskerCommand = new Command(async () => await SwitchToTasker());
            NavigateToProfileCommand = new Command(async () => await NavigateToProfile());
            CreateNewTaskCommand = new Command(async () => await CreateNewTask());
            ViewApplicantsCommand = new Command<PostedTask>(async (task) => await ViewApplicants(task));
            CancelTaskCommand = new Command<PostedTask>(async (task) => await CancelTask(task));
           

            // Load initial data
            LoadAndSaveTaskCategories();
            LoadPostedTasks();


            // Start listening for task changes
            ListenForTaskChanges();

            MessagingCenter.Subscribe<TaskCompletionViewModel, string>(this, "TaskCompleted", (sender, taskId) =>
            {
                RemoveTaskFromUI(taskId);
            });
        }

        private async Task ListenForTaskChanges()
        {
            var observable = _firebaseClient
                .Child("tasks")
                .AsObservable<PostedTask>()
                .Subscribe(d =>
                {
                    if (d.Object != null)
                    {
                        // Update the task in the local collection
                        var task = PostedTasks.FirstOrDefault(t => t.Task == d.Object.Task);
                        if (task != null)
                        {
                            task.IsBooked = d.Object.IsBooked;
                            task.HasNewApplicant = d.Object.HasNewApplicant;
                            // Update the UI or notify the user about the new applicant
                            Console.WriteLine($"Task updated: {task.Task}, HasNewApplicant: {task.HasNewApplicant}");
                        }
                    }
                });

            // Keep the subscription active
            await Task.Delay(-1);
        }

        private async Task SwitchToTaskee()
        {
            await Shell.Current.GoToAsync("///TasksMenu");
        }

        private async Task SwitchToTasker()
        {
            // This is the current page, so no navigation needed
        }

        private async Task NavigateToProfile()
        {
            await Shell.Current.GoToAsync("//ProfilePage");
        }

        private async Task CreateNewTask()
        {
            await Shell.Current.GoToAsync("TaskFormPage");
        }

        private async Task ViewApplicants(PostedTask task)
        {
            if (task == null)
            {
                Console.WriteLine("Task is null in ViewApplicants");
                return;
            }

            try
            {
                Console.WriteLine($"ViewApplicants called for task: {task.Task}");
                Console.WriteLine($"Firebase key: {task.FirebaseKey}");

                if (!string.IsNullOrEmpty(task.FirebaseKey))
                {
                    var navigationParameter = $"TaskApplicantsPage?TaskKey={Uri.EscapeDataString(task.FirebaseKey)}";
                    Console.WriteLine($"Navigating to: {navigationParameter}");
                    await Shell.Current.GoToAsync(navigationParameter);
                }
                else
                {
                    Console.WriteLine("FirebaseKey is null or empty");
                    await Shell.Current.DisplayAlert("Error", "Could not find task details", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ViewApplicants: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to fetch task details. Please try again.", "OK");
            }
        }
        private async Task CancelTask(PostedTask task)
        {
            if (task == null) return;

            try
            {
                // Mark task as canceled
                task.Status = "Cancelled";

                // Update task in Firebase
                await _firebaseClient
                .Child("tasks")
                .Child(task.FirebaseKey)  // Use Firebase key instead of task name
                .PutAsync(task);


                // Remove from local collection
                PostedTasks.Remove(task);

                // Recalculate total expenses
                CalculateTotalExpenses();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error canceling task: {ex.Message}");
            }
        }


       

        private async Task LoadAndSaveTaskCategories()
        {
            // Create an instance of FirebaseClient
            var firebase = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");

            // Initialize TaskCategories
            TaskCategories = new ObservableCollection<TaskCategory>
    {
        new TaskCategory { Name = "Home Repair", Icon = "kind.png" },
        new TaskCategory { Name = "Cleaning", Icon = "cleaning.png" },
        new TaskCategory { Name = "Moving", Icon = "moving.png" },
        new TaskCategory { Name = "Gardening", Icon = "gardening.png" }
    };

            // Save TaskCategories to Firebase
            foreach (var category in TaskCategories)
            {
                await firebase
                    .Child("TaskCategories") // The path in the Firebase database
                    .PostAsync(category);    // Save each category
            }
        }

        private async void LoadPostedTasks()
        {
            try
            {
                var tasks = await _firebaseClient
                    .Child("tasks")
                    .OnceAsync<PostedTask>();

                PostedTasks = new ObservableCollection<PostedTask>(
                    tasks
                        .Where(t => t.Object.Status != "Cancelled" && t.Object.Status != "Completed") // Exclude completed tasks
                        .Select(t => new PostedTask
                        {
                            Task = t.Object.Task,
                            Location = t.Object.Location,
                            Price = t.Object.Price,
                            Status = t.Object.Status,
                            IsBooked = t.Object.IsBooked,
                            HasNewApplicant = t.Object.HasNewApplicant,
                            Applicants = t.Object.Applicants ?? new List<string>(),
                            SelectedApplicant = t.Object.SelectedApplicant,
                            FirebaseKey = t.Key
                        })
                );

                CalculateTotalExpenses();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
            }
        }

        private void CalculateTotalExpenses()
        {
            // Filter out cancelled tasks before calculating the total
            TotalExpenses = PostedTasks?
                .Where(task => task.Status != "Cancelled") // Exclude cancelled tasks
                .Sum(task => task.Price) ?? 0;
        }
        private void RemoveTaskFromUI(string taskId)
        {
            var taskToRemove = PostedTasks.FirstOrDefault(t => t.FirebaseKey == taskId);
            if (taskToRemove != null)
            {
                PostedTasks.Remove(taskToRemove);
                CalculateTotalExpenses(); // Recalculate expenses after removing task
            }
        }
    }


    public class TaskCategory
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class PostedTask
    {
        public string Task { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; } // Added Price for calculating total expenses
        public string Status { get; set; }
        public bool IsBooked { get; set; }
        public bool HasNewApplicant { get; set; }
        public List<string> Applicants { get; set; } = new List<string>(); // List of applicants
        public string SelectedApplicant { get; set; } // Track the selected applicant
        public string FirebaseKey { get; set; }
        public DateTime AssignedDate { get; internal set; }
        public string TaskerEmail { get; set; }
        public bool TaskerConfirmed { get; set; } // Tasker confirmation
        public bool TaskeeConfirmed { get; set; } // Taskee confirmation
    }
}
