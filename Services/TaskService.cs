using System.Collections.Generic;
using System.Linq;
using taskConnectProject.Models;
using taskConnectProject.ViewModel;
using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerativeAI.Requests;
namespace taskConnectProject.Services
{
    public class TaskService
    {
        private readonly FirebaseClient _firebaseClient;
        public TaskService()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/"); // Firebase URL
        }
        // Fetch categories from Firebase 
        public async Task<List<TaskCategory>> GetCategoriesAsync()
        {
            var categories = await _firebaseClient
                .Child("TaskCategories")
                .OnceAsync<TaskCategory>();
            return categories.Select(c => new TaskCategory
            {
                //Id = c.Object.Id,  assuming your TaskCategory has an Id
                Name = c.Object.Name,
                Icon = c.Object.Icon
            }).ToList();
        }
        // Fetch available tasks from Firebase (assuming tasks are stored in a "tasks" node)
        public async Task<List<AvailableTask>> GetAvailableTasksAsync()
        {
            var tasks = await _firebaseClient
                .Child("tasks")
                .OnceAsync<AvailableTask>();
            return tasks.Select(t => new AvailableTask
            {

                Task =t.Object.Task,
                Location = t.Object.Location,
                Price = t.Object.Price,
                IsBooked = t.Object.IsBooked,

            }).ToList();
        }
        public async Task PostTaskAsync(AvailableTask newTask)
        {
            try
            {
                // Set the task to be not booked by default
                newTask.IsBooked = false;
                // Add the new task to the "tasks" node in Firebase
                var createdTask = await _firebaseClient
                    .Child("tasks")
                    .PostAsync(newTask);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting task: {ex.Message}");
                throw new Exception("Failed to post task. Please try again.");
            }
        }
        // Retrieve a task by its ID from Firebase
        public async Task<AvailableTask> GetTaskByIdAsync(string Task)
        {
            var task = await _firebaseClient
                .Child("tasks")
                .OrderByKey()
                .EqualTo(Task)
                .OnceAsync<AvailableTask>();
            return task.FirstOrDefault()?.Object;

        }
        public async Task<List<Models.TaskModel>> GetPostedTasksAsync(string Email)
        {
            var tasks = await _firebaseClient
                .Child("tasks") //  "tasks" node contains all tasks
                .OrderBy("createdBy")
                .EqualTo(Email) // Filter by the current user
                .OnceAsync<Models.TaskModel>();
            return tasks.Select(t => new Models.TaskModel
            {
                Task = t.Object.Task,
                Location = t.Object.Location,
                Description = t.Object.Description,
                Price = t.Object.Price,
                createdBy = t.Object.createdBy,
                Status = t.Object.Status,

            }).ToList();
        }


    }
}