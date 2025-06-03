namespace IntuitERP.Viwes;

public partial class MaenuPage : ContentPage
{


    public MaenuPage()
    {
        InitializeComponent();
        BindingContext = this;    


    }

    private void ClientesBntClicked(object sender, EventArgs e)
    {
        // Create and show a new window
        Window newWindow = new Window(new CadastrodeCidade());
        Application.Current.OpenWindow(newWindow);

    }
}