using Microsoft.Maui.ApplicationModel.Communication;
using System.Threading.Tasks;
using System.Windows.Input;
using taskConnectProject.Models;
using taskConnectProject.Services;
using taskConnectProject.ViewModel;

namespace taskConnectProject.ViewModel
{
    public class TaskBookingViewModel : BaseViewModel
    {
        private readonly BookingService _bookingService;

        private AvailableTask _taskDetails;
        public AvailableTask TaskDetails
        {
            get => _taskDetails;
            set => SetProperty(ref _taskDetails, value);
        }

        public ICommand ConfirmBookingCommand { get; }

        public TaskBookingViewModel()
        {
            _bookingService = new BookingService();
            ConfirmBookingCommand = new Command(OnConfirmBooking);
        }

        public void Initialize(AvailableTask task)
        {
            TaskDetails = task;
        }

        private async void OnConfirmBooking()
        {
            if (TaskDetails == null)
            {
                await Shell.Current.DisplayAlert("Error", "No task selected.", "OK");
                return;
            }

            try
            {
                string email = "ndzal@gmail"; // Replace with actual user email from authentication or settings
                await _bookingService.BookTaskAsync(TaskDetails, email);

                // Navigate back to MyTasksPage
                await Shell.Current.GoToAsync("//MyTasksPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Booking failed: {ex.Message}", "OK");
            }
        }
    }

}
