// MainPage.xaml.cs (or a new ContentPage code-behind)
using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace IntuitERP.Viwes.Modals;

public partial class ModalPicker: ContentPage
{
    // The TaskCompletionSource now works with 'object' internally.
    private readonly TaskCompletionSource<object> _taskCompletionSource;

    // The list of all items, stored as a non-generic IEnumerable.
    private readonly IEnumerable _allItems;

    private object _selectedItem;

    public ModalPicker(string title, IEnumerable items)
    {
        InitializeComponent();

        Title = title;
        _allItems = items;
        _taskCompletionSource = new TaskCompletionSource<object>();

        // The ListView's ItemsSource is set directly with the non-generic collection.
        SearchResultsListView.ItemsSource = _allItems;
    }

    
    public static async Task<T> Show<T>(INavigation navigation, string title, IEnumerable<T> items)
    {
        // 1. Create an instance of the non-generic ModalPicker.
        var modal = new ModalPicker(title, items);

        // 2. Push the modal onto the navigation stack.
        await navigation.PushModalAsync(modal);

        // 3. Await the internal TaskCompletionSource. This task will only complete 
        //    when the user selects an item or cancels.
        var result = await modal._taskCompletionSource.Task;

        // 4. Cast the object result back to the original generic type 'T'.
        return (T)result;
    }

    /// <summary>
    /// Handles the TextChanged event of the SearchBar to filter the list.
    /// </summary>
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            SearchResultsListView.ItemsSource = _allItems;
        }
        else
        {
            // The filtering logic still works perfectly because it calls ToString() on each item,
            // which is available on the base 'object' type.
            SearchResultsListView.ItemsSource = _allItems
                .Cast<object>() // Cast to object to use LINQ
                .Where(item => item.ToString().ToLowerInvariant().Contains(searchText))
                .ToList();
        }
    }

    /// <summary>
    /// Handles the selection of an item in the ListView.
    /// </summary>
    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        // The SelectedItem is already an object, so no cast is needed here.
        _selectedItem = e.SelectedItem;
    }

    /// <summary>
    /// Handles the 'Select' button click.
    /// </summary>
    private async void OnSelectButtonClicked(object sender, EventArgs e)
    {
        // Set the result on the TaskCompletionSource with the selected object.
        _taskCompletionSource.SetResult(_selectedItem);

        // Close the modal.
        await Navigation.PopModalAsync();
    }

    /// <summary>
    // Handles the 'Cancel' button click.
    /// </summary>
    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        // Set a default/null result to indicate cancellation.
        _taskCompletionSource.SetResult(null);

        // Close the modal.
        await Navigation.PopModalAsync();
    }

    /// <summary>
    /// Overrides the back button behavior to ensure the task is cancelled properly.
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        // When the physical back button is pressed, the page is about to be popped by the system.
        // We must complete the task so the calling page doesn't wait forever.
        _taskCompletionSource.TrySetResult(null);

        // Allow the default back button behavior to proceed and pop the page.
        return base.OnBackButtonPressed();
    }
}