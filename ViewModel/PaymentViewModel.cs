using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using taskConnectProject.Models;
 // For Browser API

namespace taskConnectProject.ViewModel
{
    public class PaymentViewModel : INotifyPropertyChanged
    {
        private readonly FirebaseClient _firebaseClient;

        public PaymentViewModel()
        {
            _firebaseClient = new FirebaseClient("https://taskconnect-7425c-default-rtdb.firebaseio.com/");
            SubmitPaymentCommand = new Command(async () => await SubmitPaymentAsync());
            OpenPaymentLinkCommand = new Command(OpenPaymentLink);
        }

        // Properties for payment details
        private string _userId;
        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _currency;
        public string Currency
        {
            get => _currency;
            set => SetProperty(ref _currency, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Property for the payment link
        private string _paymentLink = "https://www.paypal.com/ncp/payment/RA7EQMSBUKQVN";
        public string PaymentLink
        {
            get => _paymentLink;
            set => SetProperty(ref _paymentLink, value);
        }

        // Commands
        public ICommand SubmitPaymentCommand { get; }
        public ICommand OpenPaymentLinkCommand { get; }

        // Submit payment to Firebase
        private async Task SubmitPaymentAsync()
        {
            if (string.IsNullOrWhiteSpace(UserId) || Amount <= 0 || string.IsNullOrWhiteSpace(Currency))
            {
                StatusMessage = "Please fill in all fields correctly.";
                return;
            }

            var payment = new Payment
            {
                UserId = UserId,
                Amount = Amount,
                Currency = Currency,
                Status = "Pending",
                Timestamp = DateTime.UtcNow
            };

            try
            {
                await _firebaseClient
                    .Child("payments")
                    .PostAsync(payment);
                StatusMessage = "Payment submitted successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        // Open the payment link in a browser
        private async void OpenPaymentLink()
        {
            try
            {
                await Browser.OpenAsync(PaymentLink, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening link: {ex.Message}";
            }
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
}
