using IntuitERP.Config;
using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace IntuitERP.Viwes.Search;

public partial class UsuarioSearch : ContentPage
{
    private readonly UsuarioService _usuarioService;

    public ObservableCollection<UsuarioModel> _listaUsuariosDisplay { get; set; }
    private List<UsuarioModel> _masterListaUsuarios;
    private UsuarioModel _usuarioSelecionado;
    public UsuarioSearch(UsuarioService usuarioService)
    {
        InitializeComponent();

        if (usuarioService == null)
            throw new ArgumentNullException(nameof(usuarioService));

        _usuarioService = usuarioService;

        _listaUsuariosDisplay = new ObservableCollection<UsuarioModel>();
        _masterListaUsuarios = new List<UsuarioModel>();
        UsuariosCollectionView.ItemsSource = _listaUsuariosDisplay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsuariosAsync();
        _usuarioSelecionado = null;
        UsuariosCollectionView.SelectedItem = null;
        UpdateActionButtonsState();
    }

    private async Task LoadUsuariosAsync()
    {
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            _masterListaUsuarios = new List<UsuarioModel>(usuarios.OrderBy(u => u.Usuario));
            FilterUsuarios();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading users: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível carregar a lista de usuários: {ex.Message}", "OK");
        }
    }

    private void FilterUsuarios()
    {
        string searchTerm = UsuarioSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var previouslySelectedCode = _usuarioSelecionado?.CodUsuarios;

        _listaUsuariosDisplay.Clear();
        IEnumerable<UsuarioModel> filteredList;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredList = _masterListaUsuarios;
        }
        else
        {
            filteredList = _masterListaUsuarios.Where(u =>
                u.Usuario?.ToLowerInvariant().Contains(searchTerm) ?? false
            );
        }

        foreach (var usuario in filteredList)
        {
            _listaUsuariosDisplay.Add(usuario);
        }

        if (previouslySelectedCode.HasValue)
        {
            var reselected = _listaUsuariosDisplay.FirstOrDefault(u => u.CodUsuarios == previouslySelectedCode.Value);
            if (reselected != null)
            {
                UsuariosCollectionView.SelectedItem = reselected;
            }
            else
            {
                _usuarioSelecionado = null;
                UsuariosCollectionView.SelectedItem = null;
            }
        }
        UpdateActionButtonsState();
    }

    private void UpdateActionButtonsState()
    {
        bool isSelected = _usuarioSelecionado != null;
        EditarUsuarioButton.IsEnabled = isSelected;
        ExcluirUsuarioButton.IsEnabled = isSelected;
    }

    private void UsuariosCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _usuarioSelecionado = e.CurrentSelection.FirstOrDefault() as UsuarioModel;
        UpdateActionButtonsState();
    }

    private void UsuarioSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterUsuarios();
    }

    private async void NovoUsuarioButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var configurator = new Configurator();
            IDbConnection newPageConnection = configurator.GetMySqlConnection();
            if (newPageConnection.State == ConnectionState.Closed) newPageConnection.Open();

            var usuarioServiceForNewPage = new UsuarioService(newPageConnection);

            // Assuming CadastrodeUsuario's constructor can handle a null model for a new entry
            await Navigation.PushAsync(new CadastrodeUsuario(usuarioServiceForNewPage, 0));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to New User: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de cadastro: {ex.Message}", "OK");
        }
    }

    private async void EditarUsuarioSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_usuarioSelecionado == null)
        {
            await DisplayAlert("Nenhum Usuário Selecionado", "Por favor, selecione um usuário para editar.", "OK");
            return;
        }

        try
        {
            var configurator = new Configurator();
            IDbConnection editPageConnection = configurator.GetMySqlConnection();
            if (editPageConnection.State == ConnectionState.Closed) editPageConnection.Open();

            var usuarioServiceForEditPage = new UsuarioService(editPageConnection);

            // You'll need to adapt CadastrodeUsuario to accept a UsuarioModel
            await Navigation.PushAsync(new CadastrodeUsuario(usuarioServiceForEditPage, _usuarioSelecionado.CodUsuarios));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to Edit User: {ex.ToString()}");
            await DisplayAlert("Erro", $"Não foi possível abrir a tela de edição: {ex.Message}", "OK");
        }
    }

    private async void ExcluirUsuarioSelecionadoButton_Clicked(object sender, EventArgs e)
    {
        if (_usuarioSelecionado == null)
        {
            await DisplayAlert("Nenhum Usuário Selecionado", "Por favor, selecione um usuário para excluir.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Confirmar Exclusão",
            $"ATENÇÃO: Esta ação é PERMANENTE e não pode ser desfeita.\n\nTem certeza que deseja excluir o usuário '{_usuarioSelecionado.Usuario}'?",
            "Sim, Excluir Permanentemente", "Não");

        if (confirm)
        {
            try
            {
                int rowsAffected = await _usuarioService.DeleteAsync(_usuarioSelecionado.CodUsuarios);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Sucesso", "Usuário excluído permanentemente.", "OK");

                    _masterListaUsuarios.RemoveAll(u => u.CodUsuarios == _usuarioSelecionado.CodUsuarios);
                    FilterUsuarios();

                    _usuarioSelecionado = null;
                    UsuariosCollectionView.SelectedItem = null;
                    UpdateActionButtonsState();
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível excluir o usuário.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error excluding user: {ex.ToString()}");
                // Check for foreign key constraint violation
                if (ex.Message.Contains("foreign key constraint fails"))
                {
                    await DisplayAlert("Erro de Exclusão", "Não é possível excluir este usuário pois ele pode estar associado a outras operações.", "OK");
                }
                else
                {
                    await DisplayAlert("Erro", $"Ocorreu um erro ao excluir o usuário: {ex.Message}", "OK");
                }
            }
        }
    }
}