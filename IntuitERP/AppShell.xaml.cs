using IntuitERP.Viwes;

namespace IntuitERP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MaenuPage), typeof(MaenuPage));
        }
    }
}
