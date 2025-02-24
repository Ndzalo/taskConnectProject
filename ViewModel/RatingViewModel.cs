using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Windows.Input;
using taskConnectProject.Models;
using taskConnectProject.Views;

namespace taskConnectProject.ViewModel
{
    public class RatingViewModel : BaseViewModel
    {
        private readonly FirebaseClient _firebaseClient;
        private string _taskId;
        private string _toUserEmail;
        private PostedTask _task;

        private string _taskName;
        public string TaskName
        {
            get => _taskName;
            set => SetProperty(ref _taskName, value);
        }

        private int _selectedRating;
        public int SelectedRating
        {
            get => _selectedRating;
            set => SetProperty(ref _selectedRating, value);
        }

        private string _comment;
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        public ICommand RateCommand { get; }
        public ICommand SubmitRatingCommand { get; }

        public RatingViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            RateCommand = new Command<string>(OnRate);
            SubmitRatingCommand = new Command(async () => await SubmitRating());
        }

        public async Task Initialize(string taskId, string toUserEmail)
        {
            _taskId = taskId;
            _toUserEmail = toUserEmail;
            await LoadTask();
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
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to load task details.", "OK");
            }
        }

        private string _star1Source = "star_empty.png";
        public string Star1Source
        {
            get => _star1Source;
            set => SetProperty(ref _star1Source, value);
        }

        private string _star2Source = "star_empty.png";
        public string Star2Source
        {
            get => _star2Source;
            set => SetProperty(ref _star2Source, value);
        }

        private string _star3Source = "star_empty.png";
        public string Star3Source
        {
            get => _star3Source;
            set => SetProperty(ref _star3Source, value);
        }

        private string _star4Source = "star_empty.png";
        public string Star4Source
        {
            get => _star4Source;
            set => SetProperty(ref _star4Source, value);
        }

        private string _star5Source = "star_empty.png";
        public string Star5Source
        {
            get => _star5Source;
            set => SetProperty(ref _star5Source, value);
        }

        private void OnRate(string rating)
        {
            if (int.TryParse(rating, out int ratingValue))
            {
                SelectedRating = ratingValue;

                // Update star images based on the selected rating
                Star1Source = ratingValue >= 1 ? "star_filled.jpg" : "star_empty.png";
                Star2Source = ratingValue >= 2 ? "star_filled.jpg" : "star_empty.png";
                Star3Source = ratingValue >= 3 ? "star_filled.jpg" : "star_empty.png";
                Star4Source = ratingValue >= 4 ? "star_filled.jpg" : "star_empty.png";
                Star5Source = ratingValue >= 5 ? "star_filled.jpg" : "star_empty.png";
            }
        }

        private async Task SubmitRating()
        {
            if (SelectedRating == 0)
            {
                await Shell.Current.DisplayAlert("Error", "Please select a rating", "OK");
                return;
            }

            var rating = new Rating
            {
                Id = Guid.NewGuid().ToString(),
                TaskId = _taskId,
                FromUserEmail = "naomi@gmail.com",
                Stars = SelectedRating,
                Comment = Comment,
                CreatedAt = DateTime.UtcNow,
                TaskName = TaskName
            };

            // Submit rating to Firebase
            await _firebaseClient.Child("ratings").PostAsync(rating);

            // Show success message
            await Shell.Current.DisplayAlert("Success", "Rating submitted successfully!", "OK");

            // Navigate back to TaskCompletionPage
            await Shell.Current.GoToAsync("///TasksMenu");
        }

    }
}