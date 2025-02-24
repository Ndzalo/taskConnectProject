using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskConnectProject.ViewModel;
using System.Threading.Tasks;
using taskConnectProject.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace taskConnectProject.Services
{
    public class BookingService
    {
        private readonly FirebaseClient _firebase;

        public BookingService()
        {
            _firebase = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
        }

        public async Task BookTaskAsync(AvailableTask task, string email)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            try
            {
                // Fetch the task from Firebase
                var taskToUpdate = (await _firebase
                    .Child("Tasks")
                    .OnceAsync<PostedTask>())
                    .FirstOrDefault(t => t.Object.Task == task.Task);

                if (taskToUpdate != null)
                {
                    // Update the task status and add the applicant
                    taskToUpdate.Object.IsBooked = true;
                    taskToUpdate.Object.Applicants.Add(email);

                    // Save the updated task to Firebase
                    await _firebase
                        .Child("Tasks")
                        .Child(taskToUpdate.Key)
                        .PutAsync(taskToUpdate.Object);

                    // Show a confirmation message
                    await Application.Current.MainPage.DisplayAlert("Task Booked",
                        $"You have successfully booked the task: {task.Task}", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Task not found. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error booking task: {ex.Message}");
                throw;
            }
        }
    }
}


