using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskConnectProject.Models;

namespace taskConnectProject.ViewModel
{
    public class RatingsPageViewModel : BaseViewModel
    {
        private readonly FirebaseClient _firebaseClient;
        private ObservableCollection<Rating> _ratings;

        public ObservableCollection<Rating> Ratings
        {
            get => _ratings;
            set => SetProperty(ref _ratings, value);
        }

        public RatingsPageViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            LoadRatings();
        }

        private async void LoadRatings()
        {
            try
            {
                // Fetch all ratings from Firebase
                var ratings = await _firebaseClient
                    .Child("ratings")
                    .OnceAsync<Rating>();

                // Populate the ObservableCollection with ratings
                Ratings = new ObservableCollection<Rating>(ratings.Select(r => r.Object).ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ratings: {ex.Message}");
            }
        }
    }

}
