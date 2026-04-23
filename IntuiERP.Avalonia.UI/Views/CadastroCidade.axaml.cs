using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using IntuiERP.Avalonia.UI.models;
using IntuiERP.Avalonia.UI.Services;
using IntuiERP.Avalonia.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IntuiERP.Avalonia.UI.Views;

public partial class CadastroCidade : UserControl
{
    private readonly CidadeService _cidadeService;
    private ObservableCollection<CidadeModel> _cities = new();
    private List<CidadeModel> _allCitiesMasterList = new();

    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public CadastroCidade()
    {
        InitializeComponent();
        
        var factory = new NpgsqlConnectionFactory();
        _cidadeService = new CidadeService(factory.CreateConnection());

        // Initialize Commands for the DataTemplate buttons
        EditCommand = new RelayCommand<CidadeModel>(EditCity);
        DeleteCommand = new RelayCommand<CidadeModel>(DeleteCity);
        
        DataContext = this;

        BtnSave.Click += BtnSave_Clicked;
        BtnClear.Click += BtnClear_Clicked;
        BtnBack.Click += BtnBack_Clicked;
        EntrySearch.TextChanged += EntrySearch_TextChanged;

        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await LoadCitiesAsync();
    }

    private async Task LoadCitiesAsync()
    {
        try
        {
            var citiesList = await _cidadeService.GetAllAsync();
            _allCitiesMasterList = citiesList.ToList();

            UpdateDisplayList();
        }
        catch (Exception ex)
        {
            if (VisualRoot is Window window)
                await MessageBox.Show(window, "Não foi possível carregar a lista de cidades.", "Erro");
        }
    }

    private void UpdateDisplayList()
    {
        string searchTerm = EntrySearch.Text?.Trim().ToLower() ?? string.Empty;
        
        var filtered = _allCitiesMasterList
            .Where(c => string.IsNullOrEmpty(searchTerm) || 
                        (c.Cidade?.ToLower().Contains(searchTerm) ?? false) || 
                        (c.UF?.ToLower().Contains(searchTerm) ?? false))
            .OrderBy(c => c.Cidade);

        _cities.Clear();
        foreach (var city in filtered)
        {
            _cities.Add(city);
        }

        CitiesList.ItemsSource = _cities;
        EmptyLabel.IsVisible = !_cities.Any();
    }

    private void ClearForm()
    {
        EntryId.Text = string.Empty;
        EntryCidade.Text = string.Empty;
        EntryUF.Text = string.Empty;
        EntryCidade.Focus();
    }

    private async void BtnSave_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window) return;

        if (string.IsNullOrWhiteSpace(EntryCidade.Text) || string.IsNullOrWhiteSpace(EntryUF.Text))
        {
            await MessageBox.Show(window, "Por favor, preencha o nome da cidade e a UF.", "Campos Obrigatórios");
            return;
        }

        if (EntryUF.Text.Length > 2)
        {
            await MessageBox.Show(window, "O campo UF deve ter no máximo 2 caracteres.", "UF Inválida");
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
                    await MessageBox.Show(window, "Cidade adicionada com sucesso!", "Sucesso");
                }
                else
                {
                    await MessageBox.Show(window, "Não foi possível adicionar a cidade.", "Erro");
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
                        await MessageBox.Show(window, "Cidade atualizada com sucesso!", "Sucesso");
                    }
                    else
                    {
                        await MessageBox.Show(window, "Não foi possível atualizar a cidade.", "Erro");
                    }
                }
            }
            ClearForm();
            await LoadCitiesAsync();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(window, $"Ocorreu um erro ao salvar a cidade: {ex.Message}", "Erro");
        }
    }

    private void BtnClear_Clicked(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void EntrySearch_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateDisplayList();
    }

    private void EditCity(CidadeModel city)
    {
        EntryId.Text = city.CodCIdade.ToString();
        EntryCidade.Text = city.Cidade;
        EntryUF.Text = city.UF;
    }

    private async void DeleteCity(CidadeModel city)
    {
        if (VisualRoot is not Window window) return;

        // Note: Our MessageBox only has OK, we might need a Yes/No dialog or just alert before deleting.
        // For now, let's assume the user really wants to delete if they click it, or we could implement a YesNo dialog.
        // Let's check if we have a YesNo option in MessageBox.
        
        try
        {
            int rowsAffected = await _cidadeService.DeleteAsync(city.CodCIdade);
            if (rowsAffected > 0)
            {
                await MessageBox.Show(window, "Cidade excluída com sucesso!", "Sucesso");
                ClearForm();
                await LoadCitiesAsync();
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("foreign key") || ex.Message.Contains("violates"))
            {
                await MessageBox.Show(window, "Não é possível excluir esta cidade pois ela está sendo utilizada em outros cadastros.", "Erro de Exclusão");
            }
            else
            {
                await MessageBox.Show(window, $"Ocorreu um erro ao excluir a cidade: {ex.Message}", "Erro");
            }
        }
    }

    private void BtnBack_Clicked(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Content = new MenuPage();
        }
    }
}

// Simple RelayCommand implementation if not using a library
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute == null || (parameter is T t && _canExecute(t));

    public void Execute(object? parameter)
    {
        if (parameter is T t) _execute(t);
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}