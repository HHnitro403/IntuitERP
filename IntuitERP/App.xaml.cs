using QuestPDF.Infrastructure;

namespace IntuitERP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}