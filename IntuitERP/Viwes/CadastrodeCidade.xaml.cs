using IntuitERP.models;
using IntuitERP.Services;
using System.Collections.ObjectModel;

namespace IntuitERP.Viwes;

public partial class CadastrodeCidade : ContentPage
{
    private readonly CidadeService _cidadeService;
    private ObservableCollection<CidadeModel> _cities;
    private List<CidadeModel> _allCitiesMasterList; // For client-side filtering
    public CadastrodeCidade(CidadeService cidadeService)
    {
        InitializeComponent();
        _cidadeService = cidadeService;

        _cities = new ObservableCollection<CidadeModel>();
        _allCitiesMasterList = new List<CidadeModel>();
        CitiesList.ItemsSource = _cities;

        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCitiesAsync();
    }

    private async Task LoadCitiesAsync()
    {
        try
        {
            var citiesList = await _cidadeService.GetAllAsync();
            _allCitiesMasterList = new List<CidadeModel>(citiesList);

            _cities.Clear();
            foreach (var city in _allCitiesMasterList.OrderBy(c => c.Cidade)) // Optional: Order by name
            {
                _cities.Add(city);
            }
        }
        catch (Exception ex)
        {
            // Log the exception (e.g., using a logging framework)
            Console.WriteLine($"Error loading cities: {ex.Message}");
            await DisplayAlert("Erro", "Não foi possível carregar a lista de cidades.", "OK");
        }
    }

    private void ClearForm()
    {
        EntryId.Text = string.Empty; // It's disabled, but good to clear
        EntryCidade.Text = string.Empty;
        EntryUF.Text = string.Empty;
        EntryCidade.Focus(); // Set focus to the first editable field
    }

    private async void BtnSave_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryCidade.Text) || string.IsNullOrWhiteSpace(EntryUF.Text))
        {
            await DisplayAlert("Campos Obrigatórios", "Por favor, preencha o nome da cidade e a UF.", "OK");
            return;
        }

        if (EntryUF.Text.Length > 2)
        {
            await DisplayAlert("UF Inválida", "O campo UF deve ter no máximo 2 caracteres.", "OK");
            return;
        }

        var cidadeModel = new CidadeModel
        {
            Cidade = EntryCidade.Text.Trim(),
            UF = EntryUF.Text.Trim().ToUpper()
        };

        try
        {
            if (string.IsNullOrWhiteSpace(EntryId.Text)) // New city
            {
                int newCityId = await _cidadeService.InsertAsync(cidadeModel);
                if (newCityId > 0)
                {
                    await DisplayAlert("Sucesso", "Cidade adicionada com sucesso!", "OK");
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível adicionar a cidade.", "OK");
                }
            }
            else // Existing city (Update)
            {
                if (int.TryParse(EntryId.Text, out int cityId))
                {
                    cidadeModel.CodCIdade = cityId;
                    int rowsAffected = await _cidadeService.UpdateAsync(cidadeModel);
                    if (rowsAffected > 0)
                    {
                        await DisplayAlert("Sucesso", "Cidade atualizada com sucesso!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Não foi possível atualizar a cidade. Cidade não encontrada ou dados inalterados.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Erro", "Código da cidade inválido para atualização.", "OK");
                    return;
                }
            }
            ClearForm();
            await LoadCitiesAsync(); // Refresh the list
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving city: {ex.Message}");
            await DisplayAlert("Erro", $"Ocorreu um erro ao salvar a cidade: {ex.Message}", "OK");
        }
    }

    private void BtnClear_Clicked(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void BtnSearch_Clicked(object sender, EventArgs e)
    {
        string searchTerm = EntrySearch.Text?.Trim().ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            // If search is cleared, show all cities
            _cities.Clear();
            var cities = _allCitiesMasterList.OrderBy(c => c.Cidade);
            if (cities.Any())
            {
                _cities.Clear();
                foreach (var city in cities)
                {
                    _cities.Add(city);
                }
            }
            else
            {
                DisplayAlert("Atenção","Nenhum Registro Encontrado","OK");
            }
        }
        else
        {
            var filteredCities = _allCitiesMasterList
                .Where(c => c.Cidade.ToLower().Contains(searchTerm) ||
                            c.UF.ToLower().Contains(searchTerm))
                .OrderBy(c => c.Cidade)
                .ToList();

            if (filteredCities.Any())
            {
                _cities.Clear();
                foreach (var city in filteredCities)
                {
                    _cities.Add(city);
                }
            }


        }
    }

    // Event handler for the "Editar" button within the CollectionView's DataTemplate
    private void BtnEdit_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CidadeModel cityToEdit)
        {
            EntryId.Text = cityToEdit.CodCIdade.ToString();
            EntryCidade.Text = cityToEdit.Cidade;
            EntryUF.Text = cityToEdit.UF;
        }
    }

    // Event handler for the "Excluir" button within the CollectionView's DataTemplate
    private async void BtnDelete_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CidadeModel cityToDelete)
        {
            bool confirm = await DisplayAlert("Confirmar Exclusão", $"Tem certeza que deseja excluir a cidade: {cityToDelete.Cidade} - {cityToDelete.UF}?", "Sim", "Não");
            if (confirm)
            {
                try
                {
                    int rowsAffected = await _cidadeService.DeleteAsync(cityToDelete.CodCIdade);
                    if (rowsAffected > 0)
                    {
                        await DisplayAlert("Sucesso", "Cidade excluída com sucesso!", "OK");
                        ClearForm(); // Clear form in case the deleted item was being edited
                        await LoadCitiesAsync(); // Refresh list
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Não foi possível excluir a cidade. Cidade não encontrada.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting city: {ex.Message}");
                    // Check for foreign key constraint violation (this depends on your DB and error codes)
                    // For MySQL, error code 1451 often indicates a foreign key constraint failure.
                    if (ex.Message.Contains("foreign key constraint fails")) // Simplified check
                    {
                        await DisplayAlert("Erro de Exclusão", "Não é possível excluir esta cidade pois ela está sendo utilizada em outros cadastros (ex: Clientes, Fornecedores).", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Erro", $"Ocorreu um erro ao excluir a cidade: {ex.Message}", "OK");
                    }
                }
            }
        }
    }
}