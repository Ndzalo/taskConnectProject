using GenerativeAI.Models;
using Microsoft.Extensions.Logging;
using taskConnectProject.Models;
using taskConnectProject.Views;
using taskConnectProject.ViewModel;
using Microsoft.Extensions.Options;

namespace taskConnectProject
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG

            // Register ViewModels

            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<HomePageViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<NotificationsViewModel>();
            builder.Services.AddTransient<PaymentViewModel>();
            builder.Services.AddTransient<ProfilePageViewModel>();
            builder.Services.AddTransient<RatingViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<TaskApplicantsViewModel>();
            builder.Services.AddTransient<TaskBookingViewModel>();
            builder.Services.AddTransient<TaskCompletionViewModel>();
            builder.Services.AddTransient<TaskerNotificationsViewModel>();
            builder.Services.AddTransient<TaskerPageViewModel>();
            builder.Services.AddTransient<TaskViewModel>();
            builder.Services.AddTransient<WelcomePageViewModel>();
            builder.Services.AddTransient<HelpViewModel>();
            builder.Services.AddTransient<PrivacySettingsViewModel>();


            // Register all pages
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ForgotPasswordPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<myTasksPage>();
            builder.Services.AddTransient<NotificationsPage>();
            //builder.Services.AddTransient<Payment>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<RatingPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<TaskApplicantsPage>();
            builder.Services.AddTransient<TaskBookingPage>();
            builder.Services.AddTransient<TaskCompletionPage>();
            builder.Services.AddTransient<TaskerMenu>();
            builder.Services.AddTransient<TaskerNotificationsPage>();
            builder.Services.AddTransient<TaskFormPage>();
            // builder.Services.AddTransient<TaskMenu>();
            builder.Services.AddTransient<WelcomePage>();
            builder.Services.AddTransient<PrivacySettingsPage>();
            builder.Services.AddTransient<HelpPage>();
            builder.Services.AddTransient<RatingPage1>();
            builder.Services.AddTransient<RatingPage>();
            builder.Services.AddTransient<TaskeeCompletionPage>();
            builder.Logging.AddDebug();
#endif
            //gemini configuration
            builder.Services.Configure<GoogleGeminiConfig>(options =>
            {
                options.ApiKey = "AIzaSyBytyNsFcJWAEqoUJ6hSIMfJeQI94eFz-Q";
            });
            //gemini service registration
            builder.Services.AddTransient<ChatViewModel>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<GoogleGeminiConfig>>();
                return new ChatViewModel(config);
            });

            return builder.Build();
        }
    }
}
