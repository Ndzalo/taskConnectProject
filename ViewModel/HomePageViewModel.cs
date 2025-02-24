using Firebase.Database;
using Firebase.Database.Query;
using GenerativeAI.Requests;
using System;
using Firebase.Auth;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using taskConnectProject.Models;
using taskConnectProject.Services;
using taskConnectProject.Views;


namespace taskConnectProject.ViewModel
{
    public class HomePageViewModel : BaseViewModel


    {
       
       
        private readonly TaskService _taskService;
        private readonly FirebaseClient _firebaseClient;
       
        private FirebaseAuthProvider _authProvider;

        private ObservableCollection<TaskCategory> _categories;
        public ObservableCollection<TaskCategory> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private ObservableCollection<AvailableTask> _filteredTasks;
        public ObservableCollection<AvailableTask> FilteredTasks
        {
            get => _filteredTasks;
            set => SetProperty(ref _filteredTasks, value);
        }

        private ObservableCollection<AvailableTask> _allTasks;
        public ObservableCollection<AvailableTask> AllTasks
        {
            get => _allTasks;
            set => SetProperty(ref _allTasks, value);
        }

        private TaskCategory _selectedCategory;
        public TaskCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    // Debug message for category change
                    Console.WriteLine($"Category selected: {value?.Name ?? "None"}");

                    // Filter tasks based on booking status (no category filtering anymore)
                    FilterTasksByBookingStatus();
                }
            }
        }
      


        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        private string _loggedInUserEmail;
        public string LoggedInUserEmail
        {
            get => _loggedInUserEmail;
            set
            {
                _loggedInUserEmail = value;
                OnPropertyChanged(nameof(LoggedInUserEmail)); // Notify UI if using MVVM
            }
        }

        private readonly ProfilePageViewModel _profileViewModel;
        private string _currentUserName;

        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public ICommand SwitchToTaskeeCommand { get; }
        public ICommand SwitchToTaskerCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand GetStartedCommand { get; }
        public ICommand BookTaskCommand { get; }
        public ICommand NavigateToMyTasksCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand NavigateToNotificationsCommand { get; }



        // Constructor
        public HomePageViewModel()
        {
            LoadUserEmail(); // Call async method to set LoggedInUserEmail
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            _taskService = new TaskService();
            // Fetch and store logged-in user's email
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCDUjGoGt8py3ok6DxPLlf1YtbrotXwZPU"));
           // LoggedInUserEmail = GetCurrentUserEmail();

            Console.WriteLine($"Logged-in user email: {LoggedInUserEmail}");


            // Initialize commands
            SwitchToTaskeeCommand = new Command(async () => await SwitchToTaskee());
            SwitchToTaskerCommand = new Command(async () => await SwitchToTasker());
            NavigateToProfileCommand = new Command(async () => await NavigateToProfile());
            GetStartedCommand = new Command(async () => await GetStarted());
            BookTaskCommand = new Command<AvailableTask>(async (task) => await BookTask(task));
            //BookTaskCommand = new Command<AvailableTask>(OnBookTask);
             //NavigateToMyTasksCommand = new Command(async () => await NavigateToMyTasks());
            ResetFiltersCommand = new Command(ResetFilters);
            NavigateToNotificationsCommand = new Command(async () => await NavigateToNotifications());

            // Load initial data
            LoadCategories();
            LoadTasksForTaskee();
            ListenForTaskChanges();

            _profileViewModel = new ProfilePageViewModel();
            InitializeUserData();
        }
        private async void InitializeUserData()
        {
            await _profileViewModel.LoadUserProfile();
            CurrentUserName = _profileViewModel.Name;
        }
        /* private async void OnBookTask(AvailableTask task)
         {
             if (task != null)
             {
                 // Booking logic goes here
                 // For example, navigate to the booking page with the task details
                 await Shell.Current.GoToAsync("//TaskBookingPage?Task=" + task.Task);
             }
         }*/
        private async void LoadUserEmail()
        {
            LoggedInUserEmail = await GetCurrentUserEmail();
        }

        private async Task<string> GetCurrentUserEmail()
        {
            try
            {
                // Retrieve the Firebase auth token from secure storage
                var savedAuth = await SecureStorage.GetAsync("firebase_auth_token");

                if (!string.IsNullOrEmpty(savedAuth))
                {
                    // Get user details from Firebase Auth
                    var auth = await _authProvider.GetUserAsync(savedAuth);
                    if (auth?.Email != null)
                    {
                        Console.WriteLine($"Logged-in user email: {auth.Email}");
                        return auth.Email;
                    }
                }

                Console.WriteLine("No user found in Firebase Auth.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user email: {ex.Message}");
                return null;
            }
        }




        private async Task NavigateToNotifications()
        {
            // Debug message for navigation
            Console.WriteLine("Navigating to Notifications page.");
            await Shell.Current.GoToAsync("NotificationsPage");
        }
        public async Task PostTask(AvailableTask newTask)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Ensure the task is marked as available
                newTask.IsBooked = false;


                // Save to Firebase
                await _taskService.PostTaskAsync(newTask);

                // Reload all tasks to ensure synchronization
                await RefreshTasks();

                ErrorMessage = "Task posted successfully!";
                Console.WriteLine("Task posted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting task: {ex.Message}");
                ErrorMessage = "Failed to post task. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // New method to refresh tasks
        public async Task RefreshTasks()
        {
            try
            {
                Console.WriteLine("Refreshing tasks...");
                var tasks = await _taskService.GetAvailableTasksAsync();

                // Update both collections
                AllTasks = new ObservableCollection<AvailableTask>(tasks);
                FilteredTasks = new ObservableCollection<AvailableTask>(
                    tasks.Where(t => !t.IsBooked)
                );

                Console.WriteLine($"Tasks refreshed. Total: {tasks.Count}, Filtered: {FilteredTasks.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing tasks: {ex.Message}");
                ErrorMessage = "Failed to refresh tasks. Please try again.";
            }
        }

        private async Task SwitchToTaskee()
        {
            // Debug message for navigation
            Console.WriteLine("Navigating to Taskee menu.");
            await Shell.Current.GoToAsync("///TasksMenu");
        }



        private async Task SwitchToTasker()
        {

            Console.WriteLine("Navigating to Tasker menu.");
            await Shell.Current.GoToAsync("/TaskerMenu");

        }
        private async Task NavigateToProfile()
        {
            // Debug message for navigation
            Console.WriteLine("Navigating to Profile page.");
            await Shell.Current.GoToAsync("ProfilePage");
        }

        private async Task GetStarted()
        {
            // Debug message for navigation
            Console.WriteLine("Navigating to Login page.");
            await Shell.Current.GoToAsync("LoginPage");
        }

        private async Task BookTask(AvailableTask task)
        {
            if (task == null || task.IsBooked) return;

            try
            {
                // Fetch all tasks from Firebase
                var tasks = await _firebaseClient
                    .Child("tasks")
                    .OnceAsync<PostedTask>();

                // Find the task to update using its unique key
                var taskToUpdate = tasks.FirstOrDefault(t => t.Key == task.FirebaseKey);

                if (taskToUpdate != null)
                {
                    // Mark task as booked
                    taskToUpdate.Object.IsBooked = true;
                    taskToUpdate.Object.HasNewApplicant = true;


                    // Add the current user (taskee) to the list of applicants
                    if (taskToUpdate.Object.Applicants == null)
                    {
                        taskToUpdate.Object.Applicants = new List<string>();
                    }
                    // Add the current user (taskee) to the list of applicants
                    taskToUpdate.Object.Applicants.Add("naomi@gmail.com"); // Replace with the actual user's email or ID

                    // Update the task in Firebase using its unique key
                    await _firebaseClient
                        .Child("tasks")
                        .Child(taskToUpdate.Key) // Use the unique key to update the task
                        .PutAsync(taskToUpdate.Object);

                    // Show a success message to the Taskee
                    await Shell.Current.DisplayAlert("Success", "You have successfully booked the task!", "OK");

                    // Refresh the task list
                    await RefreshTasks();

                    // Notify tasker about the new applicant
                    await NotifyTasker(task);

                    // Debug message for booking task
                    Console.WriteLine($"Task booked: {task.Task}");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Task not found. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error booking task: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to book the task. Please try again.", "OK");
            }
        }

        private async Task NotifyTasker(AvailableTask task)
        {
            try
            {
                // Fetch all tasks from Firebase
                var tasks = await _firebaseClient
                    .Child("tasks")
                    .OnceAsync<PostedTask>();

                // Find the task to update using its unique key
                var taskToUpdate = tasks.FirstOrDefault(t => t.Key == task.FirebaseKey);

                if (taskToUpdate != null)
                {
                    // Update the task to indicate a new applicant
                    taskToUpdate.Object.HasNewApplicant = true;

                    // Update task in Firebase using its unique key
                    await _firebaseClient
                        .Child("tasks")
                        .Child(taskToUpdate.Key) // Use the unique key to update the task
                        .PutAsync(taskToUpdate.Object);

                    Console.WriteLine("Tasker notified about the new applicant.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying tasker: {ex.Message}");
            }
        }



        // Load task categories from Firebase
        private async void LoadCategories()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                Console.WriteLine("Loading categories...");
                var categories = await _taskService.GetCategoriesAsync();
                Categories = new ObservableCollection<TaskCategory>(categories);
                ErrorMessage = string.Empty; // Clear any previous error messages
                Console.WriteLine($"Categories loaded: {categories.Count} categories.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading categories: {ex.Message}");
                ErrorMessage = "Failed to load categories. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Load available tasks for Taskee
        private async void LoadTasksForTaskee()
        {
            try
            {
                var firebase = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
                var tasks = await firebase
                    .Child("tasks")
                    .OnceAsync<AvailableTask>();

                // Filter only unbooked tasks and exclude cancelled ones
                var availableTasks = tasks
                    .Select(t => new AvailableTask
                    {
                        Task = t.Object.Task,
                        Location = t.Object.Location,
                        Price = t.Object.Price,
                        IsBooked = t.Object.IsBooked,
                        HasNewApplicant = t.Object.HasNewApplicant,
                        FirebaseKey = t.Key,
                        Status = t.Object.Status 
                    })
                    .Where(t => !t.IsBooked && t.Status != "Cancelled") 
                    .ToList();

                Console.WriteLine($"Loaded {availableTasks.Count} tasks.");
                AllTasks = new ObservableCollection<AvailableTask>(availableTasks);
                FilteredTasks = new ObservableCollection<AvailableTask>(availableTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
            }
        }



        // Filter tasks by selected category
        private void FilterTasksByBookingStatus()
        {
            if (AllTasks == null) return;

            Console.WriteLine("Filtering tasks by booking status...");

            FilteredTasks = new ObservableCollection<AvailableTask>(
                AllTasks.Where(task => !task.IsBooked && task.Status != "Cancelled") // Exclude cancelled tasks
            );

            Console.WriteLine($"Filtered tasks count: {FilteredTasks.Count}");
        }



        // Method to reset filters
        private void ResetFilters()
        {
            Console.WriteLine("Resetting filters.");
            SelectedCategory = null; // Clear the selected category
            FilteredTasks = new ObservableCollection<AvailableTask>(AllTasks.Where(task => !task.IsBooked)); // Reload all unbooked tasks
        }

        public async Task ListenForTaskChanges()
        {
            var observable = _firebaseClient
                .Child("tasks")
                .AsObservable<AvailableTask>()
                .Subscribe(d =>
                {
                    if (d.Object != null)
                    {
                        // Update the task in the local collection
                        var task = AllTasks.FirstOrDefault(t => t.Task == d.Object.Task);
                        if (task != null)
                        {
                            task.IsBooked = d.Object.IsBooked;
                            FilterTasksByBookingStatus();
                        }
                    }
                });

            // Keep the subscription active
            await Task.Delay(-1);
        }

    }

    // Supporting model classes
    public class AvailableTask
    {

       
        public string Task { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked { get; set; } // Represents if the task is already taken
        public bool HasNewApplicant { get; set; }
        public string FirebaseKey { get; set; }
        public string Status { get; set; } // Add this property


    }
}
