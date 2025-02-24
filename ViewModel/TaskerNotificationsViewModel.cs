using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using taskConnectProject.Models;
using taskConnectProject.Views;
using static taskConnectProject.ViewModel.TaskApplicantsViewModel;

namespace taskConnectProject.ViewModel
{
    public class TaskerNotificationsViewModel : BaseViewModel
    {
        private readonly FirebaseClient _firebaseClient;
        private ObservableCollection<TaskNotification> _notifications;
        public ObservableCollection<TaskNotification> Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
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

        public ICommand MarkAsReadCommand { get; }

        public TaskerNotificationsViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            Notifications = new ObservableCollection<TaskNotification>();
            MarkAsReadCommand = new Command<TaskNotification>(async (notification) => await MarkAsRead(notification));

            // Load notifications when the ViewModel is initialized
            LoadNotifications();
            ListenForNotifications();
        }

        private async void LoadNotifications()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Fetch notifications for the tasker
                var notifications = await _firebaseClient
                    .Child("notifications")
                    .OnceAsync<TaskNotification>();

                // Filter notifications for the tasker (e.g., based on tasker's email or role)
                var taskerEmail = "ndzal@gmail"; // Replace with the actual tasker's email
                var taskerNotifications = notifications
                    .Where(n => n.Object.Status == "Accepted" && n.Object.TaskerEmail == taskerEmail) // Add a TaskerEmail field to notifications
                    .Select(n => n.Object)
                    .ToList();

                Notifications = new ObservableCollection<TaskNotification>(taskerNotifications);
                ErrorMessage = string.Empty; // Clear any previous error messages
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");
                ErrorMessage = "Failed to load notifications. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ListenForNotifications()
        {
            var taskerEmail = "tasker@example.com"; // Replace with the actual tasker's email

            var observable = _firebaseClient
                .Child("notifications")
                .AsObservable<TaskNotification>()
                .Subscribe(d =>
                {
                    if (d.Object != null && d.Object.Status == "Accepted" && d.Object.TaskerEmail == taskerEmail)
                    {
                        // Add the new notification to the collection
                        Notifications.Add(d.Object);

                        // Show a local notification (optional)
                        ShowLocalNotification(d.Object);
                    }
                });

            // Keep the subscription active
            await Task.Delay(-1);
        }

        private void ShowLocalNotification(TaskApplicantsViewModel.TaskNotification notification)
        {
            // Use a local notification library (e.g., Plugin.LocalNotification) to show a notification
            Console.WriteLine($"New notification: {notification.Message}");
        }

        private async Task MarkAsRead(TaskNotification notification)
        {
            if (notification == null) return;

            try
            {
                // Mark the notification as read
                notification.IsRead = true;

                // Update the notification in Firebase
                await _firebaseClient
                    .Child("notifications")
                    .Child(notification.TaskId) // Assuming TaskId is the unique key for notifications
                    .PutAsync(notification);

                // Refresh the notifications list
                LoadNotifications();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
                ErrorMessage = "Failed to mark notification as read. Please try again.";
            }
        }

        private async Task ViewAcceptedOffer(TaskNotification notification)
        {
            if (notification == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(TaskCompletionPage)}?TaskId={notification.TaskId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to TaskCompletionPage: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to navigate. Please try again.", "OK");
            }
        }
    }
}