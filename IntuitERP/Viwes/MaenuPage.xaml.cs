using IntuitERP.Config;
using IntuitERP.Services;
using System.Data;

namespace IntuitERP.Viwes;

public partial class MaenuPage : ContentPage
{


    public MaenuPage()
    {
        InitializeComponent();
        BindingContext = this;


    }

    private async void ClientesBntClicked(object sender, EventArgs e)
    {
        try
        {
            // 1. Create an instance of Configurator
            var configurator = new Configurator();

            // 2. Get the database connection
            IDbConnection connection = configurator.GetMySqlConnection();

            // 3. Create an instance of CidadeService
            var cidadeService = new CidadeService(connection); // You'll need to create CidadeService similar to UsuarioService

            // 4. Create an instance of CadastrodeCidade
            var cadastroCidadePage = new CadastrodeCidade(cidadeService);

            // 5. Use Navigation.PushAsync to navigate
            await Navigation.PushAsync(cadastroCidadePage);
        }
        catch (Exception ex)
        {
            // Handle any potential errors during instantiation or navigation
            await DisplayAlert("Error", $"Failed to open city registration: {ex.Message}", "OK");
        }
    }


}