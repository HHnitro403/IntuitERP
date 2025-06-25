namespace IntuitERP.Viwes.Reports;

public partial class PdfViewerPage : ContentPage
{
    // Stores the path to the *original* PDF file passed in the constructor.
    private readonly string _originalPdfPath;

    // Stores the path of the *temporary* file we create for the WebView.
    private string _tempPdfPath = null;

    /// <summary>
    /// Initializes the viewer page with the path to the original PDF file.
    /// </summary>
    /// <param name="originalPdfPath">The full file path to the PDF to be loaded.</param>
    public PdfViewerPage(string originalPdfPath)
    {
        InitializeComponent();
        _originalPdfPath = originalPdfPath;
    }

    /// <summary>
    /// This method is called by the framework just before the page becomes visible.
    /// We load the PDF here to ensure all UI elements are ready.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPdfFromTempFileAsync();
    }

    /// <summary>
    /// Reads the original PDF, copies it to a unique temporary file, and loads it into the WebView.
    /// </summary>
    private async Task LoadPdfFromTempFileAsync()
    {
        // Check if the original file path is valid and exists.
        if (string.IsNullOrEmpty(_originalPdfPath) || !File.Exists(_originalPdfPath))
        {
            await DisplayAlert("Error", "The specified PDF file could not be found.", "OK");
            return;
        }

        try
        {
            // 1. Clean up any previous temp file before creating a new one.
            CleanupTempFile();

            // 2. Define a unique file path in the app's cache directory.
            string tempFileName = $"{Guid.NewGuid()}.pdf";
            _tempPdfPath = Path.Combine(FileSystem.CacheDirectory, tempFileName);

            // 3. Copy the original file to the new temporary location.
            File.Copy(_originalPdfPath, _tempPdfPath);

            // 4. (FIX) Load the temporary file into the WebView, ensuring the path is a valid URL.
            // On some platforms, especially Windows, the WebView requires a proper 'file:///' prefix
            // to correctly resolve and render local file content.
            WebViewControl.Source = new UrlWebViewSource { Url = $"file:///{_tempPdfPath}" };
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load temporary PDF file: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// This method is automatically called by the framework when the page is navigated away from.
    /// It's the perfect place to clean up our temporary file.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CleanupTempFile();
    }

    /// <summary>
    /// Safely checks for and deletes the temporary PDF file from the cache.
    /// </summary>
    private void CleanupTempFile()
    {
        try
        {
            if (!string.IsNullOrEmpty(_tempPdfPath) && File.Exists(_tempPdfPath))
            {
                File.Delete(_tempPdfPath);
                _tempPdfPath = null; // Reset the path after deletion.
            }
        }
        catch (Exception ex)
        {
            // It's not critical if a temp file can't be deleted immediately.
            // We'll write the error to the debug console instead of alerting the user.
            System.Diagnostics.Debug.WriteLine($"Failed to delete temp file: {ex.Message}");
        }
    }
}