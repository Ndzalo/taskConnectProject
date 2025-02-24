using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace taskConnectProject.ViewModel
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly FirebaseClient _firebaseClient;

        private string _task;
        private string _category;
        private string _location;
        private string _description;
        private string _statusMessage;
        private string _createdby;
        private decimal _price;
        private bool _isbooked;

        public TaskViewModel()
        {
            // Initialize Firebase client with your database URL
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");

            // Initialize commands
            SwitchToTaskeeCommand = new Command(async () => await SwitchToTaskee());
            SwitchToTaskerCommand = new Command(async () => await SwitchToTasker());
            SaveTaskCommand = new Command(async () => await SaveTaskAsync());
            LoadTasksCommand = new Command(async () => await LoadTasksAsync());
            AddNewTaskCommand = new Command(async () => await AddNewTaskAsync());

            // Initialize tasks collection
            Tasks = new ObservableCollection<TaskModel>();


        }

        // Properties for Task fields
        public string Task
        {
            get => _task;
            set => SetProperty(ref _task, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        public string createdBy
        {
            get => _createdby;
            set => SetProperty(ref _createdby, value);
        }
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }
        public bool isBooked
        {
            get => _isbooked;
            set => SetProperty(ref _isbooked, value);
        }
        private string _loggedInUserEmail;
        public string LoggedInUserEmail
        {
            get => _loggedInUserEmail;
            set => SetProperty(ref _loggedInUserEmail, value);
        }

        // Collection to hold tasks
        public ObservableCollection<TaskModel> Tasks { get; }

        // Commands
        public ICommand SwitchToTaskeeCommand { get; }
        public ICommand SwitchToTaskerCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand LoadTasksCommand { get; }

        public ICommand AddNewTaskCommand { get; }

        // Save Task to Firebase
        private async Task SaveTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Task) ||
                string.IsNullOrWhiteSpace(Category) ||
                string.IsNullOrWhiteSpace(Location) ||
                string.IsNullOrWhiteSpace(Description) ||
                string.IsNullOrWhiteSpace(createdBy) ||
                string.IsNullOrWhiteSpace(LoggedInUserEmail))
            {
                StatusMessage = "Please fill in all required fields.";
                return;
            }

            // Create a new task object
            var newTask = new TaskModel
            {
                Task = this.Task,
                Category = this.Category,
                Location = this.Location,
                Description = this.Description,
                Timestamp = DateTime.UtcNow,
                createdBy = this.createdBy,
                Price = this.Price,
                isBooked = this.isBooked,
                LoggedInUserEmail = this.LoggedInUserEmail,

            };

            try
            {
                // Save task to Firebase
                var result = await _firebaseClient
                    .Child("tasks") // This creates a "tasks" node in your database
                    .PostAsync(newTask);

                // Add the FirebaseKey to the task object
                newTask.FirebaseKey = result.Key;

                // Update the task in Firebase with the FirebaseKey
                await _firebaseClient
                    .Child("tasks")
                    .Child(result.Key)
                    .PutAsync(newTask);

                StatusMessage = "Task saved successfully!";
                ClearForm();
                await Shell.Current.GoToAsync("/TaskerMenu");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task SwitchToTaskee()
        {
            // This is the current page, so no navigation needed
            
        }

        private async Task SwitchToTasker()
        {
            await Shell.Current.GoToAsync("TaskerMenu");
        }

        // Load Tasks from Firebase
        private async Task AddNewTaskAsync()
        {
            await Shell.Current.GoToAsync("TaskFormPage");
        }
        private async Task LoadTasksAsync()
        {
            try
            {
                var tasks = await _firebaseClient
                    .Child("tasks")
                    .OnceAsync<TaskModel>();

                // Clear the existing tasks and load new ones
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(new TaskModel
                    {

                        Task = task.Object.Task,
                        Category = task.Object.Category,
                        Location = task.Object.Location,
                        Description = task.Object.Description,
                        Timestamp = task.Object.Timestamp,
                        createdBy = task.Object.createdBy,
                        Price = task.Object.Price,
                        isBooked = task.Object.isBooked,
                        FirebaseKey = task.Key,
                    



                    });
                }
                StatusMessage = "Tasks loaded successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading tasks: {ex.Message}";
            }
        }

        // Clear form fields
        private void ClearForm()
        {
            Task = string.Empty;
            Category = string.Empty;
            Location = string.Empty;
            Description = string.Empty;
            createdBy = string.Empty;


        }

        // Property change notification
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }


    }

    // Task Model
    public class TaskModel
    {

        public string Task { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }
        public string createdBy { get; set; }
        public string StatusMessage { get; set; }
        public bool isBooked { get; set; }
        public string FirebaseKey { get; set; }
        public string LoggedInUserEmail { get; set; }
    }
}