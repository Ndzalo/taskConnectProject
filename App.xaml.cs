using taskConnectProject.Views;

namespace taskConnectProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

          
                MainPage = new AppShell();
                Shell.Current.GoToAsync("WelcomePage");
            //Routing.RegisterRoute("TaskApplicantsPage", typeof(TaskApplicantsPage));

        }
    }
}
