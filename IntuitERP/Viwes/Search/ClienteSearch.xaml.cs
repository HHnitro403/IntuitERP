using IntuitERP.Config;
using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;

namespace IntuitERP.Viwes.Search
{
    public partial class ClienteSearch : ContentPage
    {
        private readonly ClienteService _clienteService;
        private readonly CidadeService _cidadeService;

        public ObservableCollection<ClienteModel> _listaClientesDisplay { get; set; }
        private List<ClienteModel> _masterListaClientes;
        private ClienteModel _clienteSelecionado; // To store the currently selected client

        public ClienteSearch(ClienteService clienteService, CidadeService cidadeService)
        {
            InitializeComponent();

            if (clienteService == null)
                throw new ArgumentNullException(nameof(clienteService), "ClienteService must be provided.");
            if (cidadeService == null)
                throw new ArgumentNullException(nameof(cidadeService), "CidadeService must be provided for navigation to edit/new.");

            _clienteService = clienteService;
            _cidadeService = cidadeService;

            _listaClientesDisplay = new ObservableCollection<ClienteModel>();
            _masterListaClientes = new List<ClienteModel>();
            ClientesCollectionView.ItemsSource = _listaClientesDisplay;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadClientesAsync();
            // Reset selection and button states when page appears
            _clienteSelecionado = null;
            if (ClientesCollectionView != null)
            {
                ClientesCollectionView.SelectedItem = null;
            }
            UpdateActionButtonsState();
        }

        private async Task LoadClientesAsync()
        {
            if (_clienteService == null)
            {
                await DisplayAlert("Erro", "Serviço de cliente não inicializado. A página não pode carregar dados.", "OK");
                return;
            }

            try
            {
                var clientes = await _clienteService.GetAllAsync();
                _masterListaClientes = new List<ClienteModel>(clientes.OrderBy(c => c.Nome));
                // This call will now use the thread-safe FilterClientes method
                FilterClientes();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading clients: {ex.ToString()}");
                await DisplayAlert("Erro", $"Não foi possível carregar a lista de clientes: {ex.Message}", "OK");
            }
        }

        private void FilterClientes()
        {
            // THE FIX: We wrap the entire UI update logic in a call to the MainThread.
            // This guarantees that the ObservableCollection, which is tied to the UI,
            // is modified on the correct thread, preventing race conditions that
            // can appear in Release builds or when running without a debugger.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                string searchTerm = ClienteSearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;

                // Preserve selection if possible
                var previouslySelectedClientCode = _clienteSelecionado?.CodCliente;

                _listaClientesDisplay.Clear();

                IEnumerable<ClienteModel> filteredList;

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    filteredList = _masterListaClientes;
                }
                else
                {
                    filteredList = _masterListaClientes.Where(c =>
                        (c.Nome?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.Email?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (c.CPF?.ToLowerInvariant().Contains(searchTerm) ?? false)
                    );
                }

                foreach (var cliente in filteredList)
                {
                    _listaClientesDisplay.Add(cliente);
                }

                // Try to re-apply selection after filtering
                if (previouslySelectedClientCode.HasValue)
                {
                    var reselectedClient = _listaClientesDisplay.FirstOrDefault(c => c.CodCliente == previouslySelectedClientCode.Value);
                    if (reselectedClient != null)
                    {
                        ClientesCollectionView.SelectedItem = reselectedClient; // This will trigger SelectionChanged and update _clienteSelecionado
                    }
                    else
                    {
                        // If previously selected item is no longer in the filtered list
                        _clienteSelecionado = null;
                        ClientesCollectionView.SelectedItem = null; // Explicitly clear
                    }
                }
                UpdateActionButtonsState(); // Update buttons based on whether an item is (still) selected
            });
        }

        private void UpdateActionButtonsState()
        {
            bool isClienteSelected = _clienteSelecionado != null;
            EditarClienteButton.IsEnabled = isClienteSelected;
            ExcluirClienteButton.IsEnabled = isClienteSelected;
        }

        private void ClientesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _clienteSelecionado = e.CurrentSelection.FirstOrDefault() as ClienteModel;
            UpdateActionButtonsState();
        }

        private void ClienteSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterClientes();
        }

        private async void NovoClienteButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var configurator = new Configurator();
                IDbConnection newPageConnection = configurator.GetMySqlConnection();

                if (newPageConnection.State == ConnectionState.Closed)
                {
                    newPageConnection.Open();
                }

                var clienteServiceForNewPage = new ClienteService(newPageConnection);
                var cidadeServiceForNewPage = new CidadeService(newPageConnection);

                await Navigation.PushAsync(new CadastrodeCliente(clienteServiceForNewPage, cidadeServiceForNewPage));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to New Client: {ex.ToString()}");
                await DisplayAlert("Erro", $"Não foi possível abrir a tela de cadastro: {ex.Message}", "OK");
            }
        }

        private async void EditarClienteSelecionadoButton_Clicked(object sender, EventArgs e)
        {
            if (_clienteSelecionado == null)
            {
                await DisplayAlert("Nenhum Cliente Selecionado", "Por favor, selecione um cliente da lista para editar.", "OK");
                return;
            }

            try
            {
                var configurator = new Configurator();
                IDbConnection editPageConnection = configurator.GetMySqlConnection();
                if (editPageConnection.State == ConnectionState.Closed)
                {
                    editPageConnection.Open();
                }

                var clienteServiceForEditPage = new ClienteService(editPageConnection);
                var cidadeServiceForEditPage = new CidadeService(editPageConnection);

                var editPage = new CadastrodeCliente(clienteServiceForEditPage, cidadeServiceForEditPage, _clienteSelecionado.CodCliente);
                await Navigation.PushAsync(editPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to Edit Client: {ex.ToString()}");
                await DisplayAlert("Erro", $"Não foi possível abrir a tela de edição: {ex.Message}", "OK");
            }
        }

        private async void ExcluirClienteSelecionadoButton_Clicked(object sender, EventArgs e)
        {
            if (_clienteSelecionado == null)
            {
                await DisplayAlert("Nenhum Cliente Selecionado", "Por favor, selecione um cliente da lista para excluir.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Confirmar Exclusão",
                $"Tem certeza que deseja excluir o cliente '{_clienteSelecionado.Nome}'?",
                "Sim, Excluir", "Não");

            if (confirm)
            {
                if (_clienteService == null)
                {
                    await DisplayAlert("Erro", "Serviço de cliente não inicializado.", "OK");
                    return;
                }
                try
                {
                    _clienteSelecionado.Ativo = false;
                    int rowsAffected = await _clienteService.UpdateAsync(_clienteSelecionado);

                    if (rowsAffected > 0)
                    {
                        await DisplayAlert("Sucesso", "Cliente marcado como excluído.", "OK");
                        // Update the item in the master list as well
                        var masterItem = _masterListaClientes.FirstOrDefault(c => c.CodCliente == _clienteSelecionado.CodCliente);
                        if (masterItem != null)
                        {
                            masterItem.Ativo = false;
                        }
                        // Refresh the displayed list (which will reflect the change via FilterClientes or direct update)
                        FilterClientes(); // This will re-filter and update the displayed list

                        // Clear selection and update buttons as the item state has changed
                        _clienteSelecionado = null;
                        ClientesCollectionView.SelectedItem = null; // Deselect
                        UpdateActionButtonsState();
                    }
                    else
                    {
                        // Revert optimistic update if DB failed
                        _clienteSelecionado.Ativo = true;
                        await DisplayAlert("Erro", "Não foi possível atualizar o status do cliente.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Revert optimistic update if exception
                    if (_clienteSelecionado != null) _clienteSelecionado.Ativo = true;
                    Console.WriteLine($"Error excluding client: {ex.ToString()}");
                    await DisplayAlert("Erro", $"Ocorreu um erro ao excluir o cliente: {ex.Message}", "OK");
                }
            }
        }
    }
}
