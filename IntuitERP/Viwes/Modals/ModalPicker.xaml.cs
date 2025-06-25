// MainPage.xaml.cs (or a new ContentPage code-behind)
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace IntuitERP.Viwes.Modals;

public partial class ModalPicker : ContentPage
{
    public ObservableCollection<object> Items { get; set; }

    public ObservableCollection<object> FilteredItems { get; set; }

    public Func<object, string, bool> SearchPredicate { get; set; }

    public Action<object> OnItemSelectedCallback { get; set; }

    public ModalPicker(object Itemlist)
    {
        InitializeComponent();

        Items = new ObservableCollection<object>();
        FilteredItems = new ObservableCollection<object>(Items);
        this.BindingContext = this;
    }

    public ModalPicker(ObservableCollection<object> initialItems, Func<object, string, bool> searchPredicate, Action<object> onItemSelectedCallback = null)
    {
        Items = initialItems;
        FilteredItems = new ObservableCollection<object>(initialItems);
        SearchPredicate = searchPredicate;
        OnItemSelectedCallback = onItemSelectedCallback;
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        FilteredItems.Clear();

        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            foreach (var item in Items)
            {
                FilteredItems.Add(item);
            }
        }
        else if (SearchPredicate != null)
        {
            var searchText = e.NewTextValue;
            var results = Items.Where(item => SearchPredicate(item, searchText));
            foreach (var item in results)
            {
                FilteredItems.Add(item);
            }
        }
        else // Fallback if no predicate is provided (e.g., relies on item.ToString())
        {
            var searchText = e.NewTextValue.ToLowerInvariant();
            var results = Items.Where(item => item.ToString().ToLowerInvariant().Contains(searchText));
            foreach (var item in results)
            {
                FilteredItems.Add(item);
            }
        }
    }

    private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem != null)
        {
            // Optionally call a callback if provided
            OnItemSelectedCallback?.Invoke(e.SelectedItem);

            // Display an alert with the selected item's ToString() representation.
            await DisplayAlert("Item Selected", $"You selected: {e.SelectedItem.ToString()}", "OK");

            ((ListView)sender).SelectedItem = null; // Deselect the item
        }
    }

    private async void OnSelectButtonClicked(object sender, EventArgs e)
    {
        object selectedItem = SearchResultsListView.SelectedItem;
        string selectedText = (selectedItem != null) ? selectedItem.ToString() : "Nothing";

        // If a callback is provided, invoke it with the selected item
        if (selectedItem != null)
        {
            OnItemSelectedCallback?.Invoke(selectedItem);
        }

        await DisplayAlert("Action", $"Select button clicked. Current selection: {selectedText}", "OK");

        // You typically want to close the modal after selection
        await Navigation.PopModalAsync();
    }

    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Action", "Cancel button clicked.", "OK");

        // You typically want to close the modal on cancel
        await Navigation.PopModalAsync();
    }
}