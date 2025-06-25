namespace IntuitERP.Viwes.Reports;

using Maui.PDFView;

public class PdfViewerPage : ContentPage
{
    private readonly PdfView pdfView;
    private readonly ActivityIndicator activityIndicator;
    private readonly byte[] pdfData;
    private readonly string fileName;

    public PdfViewerPage(byte[] pdfData, string fileName)
    {
        this.pdfData = pdfData;
        this.fileName = fileName;
        Title = fileName; // Set the page title to the report name

        // --- Create UI Elements in C# ---
        activityIndicator = new ActivityIndicator
        {
            IsRunning = true,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        pdfView = new PdfView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            IsVisible = false // Hide until the PDF is loaded
        };

        Content = new Grid
        {
            Children = { pdfView, activityIndicator }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPdf();
    }

    private async Task LoadPdf()
    {
        try
        {
            // Save the byte array to a temporary file in the cache
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pdfData);

            // Set the source of the PDF viewer
            pdfView.Uri = filePath;

            // Show the viewer and hide the activity indicator
            pdfView.IsVisible = true;
            activityIndicator.IsRunning = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load PDF: " + ex.Message, "OK");
        }
    }
}