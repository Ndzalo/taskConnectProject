using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using taskConnectProject.Models;
using static taskConnectProject.ViewModel.TaskApplicantsViewModel;

namespace taskConnectProject.ViewModel
{
    public class NotificationsViewModel : BaseViewModel
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
        public ICommand AcceptOfferCommand { get; }

        public NotificationsViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            Notifications = new ObservableCollection<TaskNotification>();
            MarkAsReadCommand = new Command<TaskNotification>(async (notification) => await MarkAsRead(notification));
            AcceptOfferCommand = new Command<TaskNotification>(async (notification) => await AcceptOffer(notification));
            // Load notifications when the ViewModel is initialized
            LoadNotifications();
            ListenForNotifications();
        }


        private async Task AcceptOffer(TaskNotification notification)
        {
            if (notification == null) return;

            try
            {
                // Fetch the task from Firebase
                var task = await _firebaseClient
                    .Child("tasks")
                    .Child(notification.TaskId)
                    .OnceSingleAsync<PostedTask>();

                if (task != null)
                {
                    // Update task status to "Accepted"
                    task.Status = "Accepted";

                    // Update the task in Firebase
                    await _firebaseClient
                        .Child("tasks")
                        .Child(notification.TaskId)
                        .PutAsync(task);

                    // Update notification status
                    notification.Status = "Accepted";

                    // Find and update the notification in Firebase
                    var notifications = await _firebaseClient
                        .Child("notifications")
                        .OnceAsync<TaskNotification>();

                    var notificationKey = notifications
                        .FirstOrDefault(n => n.Object.TaskId == notification.TaskId)?.Key;

                    if (!string.IsNullOrEmpty(notificationKey))
                    {
                        await _firebaseClient
                            .Child("notifications")
                            .Child(notificationKey)
                            .PutAsync(notification);
                    }

                    // Remove from local collection
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (Notifications.Contains(notification))
                        {
                            Notifications.Remove(notification);
                        }
                    });

                    // Show success message
                    await Shell.Current.DisplayAlert("Success", "You have accepted the offer!", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting offer: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to accept the offer. Please try again.", "OK");
            }
        }
        private async void LoadNotifications()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var notifications = await _firebaseClient
                    .Child("notifications")
                    .OnceAsync<TaskNotification>();

                var currentUserEmail = "naomi@gmail.com";
                var userNotifications = notifications
                    .Where(n => n.Object.ApplicantEmail == currentUserEmail &&
                               n.Object.Status != "Accepted") // Filter out accepted notifications
                    .Select(n => n.Object)
                    .ToList();

                Notifications = new ObservableCollection<TaskNotification>(userNotifications);
                ErrorMessage = string.Empty;
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
            var currentUserEmail = "naomi@gmail.com"; 

            var observable = _firebaseClient
                .Child("notifications")
                .AsObservable<TaskNotification>()
                .Subscribe(d =>
                {
                    if (d.Object != null && d.Object.ApplicantEmail == currentUserEmail)
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

        private void ShowLocalNotification(TaskNotification notification)
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
    }
}