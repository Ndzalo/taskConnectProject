using System.Windows.Input;
using taskConnectProject.Views;
namespace taskConnectProject
{
    public partial class AppShell : Shell
    {
        public ICommand BackCommand { get; }
        public AppShell()
        {
            InitializeComponent();

            BackCommand = new Command(async () => {
                // Check if navigation is possible
                if (Navigation.NavigationStack.Count > 1 || Navigation.ModalStack.Count > 0)
                {
                    await Shell.Current.GoToAsync("..");
                }
            });

            // Register routes for all pages
            Routing.RegisterRoute("ChatPage", typeof(ChatPage));
            Routing.RegisterRoute("ForgotPasswordPage", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("myTasksPage", typeof(myTasksPage));
            Routing.RegisterRoute("NotificationsPage", typeof(NotificationsPage));
            Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
            Routing.RegisterRoute("RatingPage", typeof(RatingPage));
            Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
            Routing.RegisterRoute("TaskApplicantsPage", typeof(TaskApplicantsPage));
            Routing.RegisterRoute("TaskBookingPage", typeof(TaskBookingPage));
            Routing.RegisterRoute("TaskCompletionPage", typeof(TaskCompletionPage));
            Routing.RegisterRoute("TaskerMenu", typeof(TaskerMenu));
            Routing.RegisterRoute("TaskerNotificationsPage", typeof(TaskerNotificationsPage));
            Routing.RegisterRoute("TaskFormPage", typeof(TaskFormPage));
            Routing.RegisterRoute("TaskMenu", typeof(TasksMenu));
            Routing.RegisterRoute("WelcomePage", typeof(WelcomePage));
            Routing.RegisterRoute("HelpPage", typeof(HelpPage));
            Routing.RegisterRoute(nameof(PrivacySettingsPage), typeof(PrivacySettingsPage));
            Routing.RegisterRoute(nameof(Payment), typeof(Payment));
            Routing.RegisterRoute(nameof(RatingPage), typeof(RatingPage));
            Routing.RegisterRoute(nameof(TaskeeCompletionPage), typeof(TaskeeCompletionPage));
            Routing.RegisterRoute(nameof(RatingPage1), typeof(RatingPage1));

           
        }
        }

    }

