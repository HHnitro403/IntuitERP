using IntuitERP.Services;

//using IntuitERP.Viwes.Reports.PDFViwer;

namespace IntuitERP.Viwes.Reports;

public partial class ReportsPage : ContentPage
{
    // These services are now provided to the page instead of created by it.
    private readonly ReportsService _reportsService;

    private readonly PdfReportService _pdfReportService;

    // MODIFIED CONSTRUCTOR:
    // The page now requires a ReportsService instance to be passed in when it's created.
    public ReportsPage(ReportsService reportsService)
    {
        InitializeComponent();

        // Store the injected service instance in the private field.
        _reportsService = reportsService;

        // The PdfReportService has no dependencies, so we can still create it here.
        // If it had dependencies, we would inject it in the constructor as well.
        _pdfReportService = new PdfReportService();
    }

    private async void OnGenerateReportClicked(object sender, EventArgs e)
    {
        if (activityIndicator.IsRunning) return;

        activityIndicator.IsRunning = true;
        var button = sender as Button;
        button.IsEnabled = false;

        try
        {
            byte[] pdfData = null;
            string reportTitle = string.Empty;
            string fileName = "report.pdf";

            // The rest of this method works exactly as before,
            // but now uses the _reportsService that was provided to the page.
            if (button == VendasReportButton)
            {
                reportTitle = "Relatório de Vendas";
                fileName = "Vendas.pdf";
                var data = await _reportsService.GetVendasReportAsync();
                var headers = new[] { "ID", "Data", "Cliente", "Vendedor", "Valor Total", "Pagamento", "Status" };
                pdfData = await _pdfReportService.GeneratePdfReport(reportTitle, headers, data.ToList());
            }
            else if (button == ComprasReportButton)
            {
                reportTitle = "Relatório de Compras";
                fileName = "Compras.pdf";
                var data = await _reportsService.GetComprasReportAsync();
                var headers = new[] { "ID", "Data", "Fornecedor", "Valor Total", "Pagamento", "Status" };
                pdfData = await _pdfReportService.GeneratePdfReport(reportTitle, headers, data.ToList());
            }
            else if (button == ProdutosReportButton)
            {
                try
                {
                    reportTitle = "Relatório de Produtos";
                    fileName = "Produtos.pdf";
                    var data = await _reportsService.GetProdutosReportAsync();
                    var headers = new[] { "ID", "Descrição", "Categoria", "Preço", "Estoque", "Fornecedor" };
                    pdfData = await _pdfReportService.GeneratePdfReport(reportTitle, headers, data.ToList());
                }
                catch (Exception ex) { } // Handle any exceptions that occur during the report gene
            }
            else if (button == ClientesReportButton)
            {
                reportTitle = "Relatório de Clientes";
                fileName = "Clientes.pdf";
                var data = await _reportsService.GetClientesReportAsync();
                var headers = new[] { "ID", "Nome", "Email", "Telefone", "CPF", "Cidade", "UF" };
                pdfData = await _pdfReportService.GeneratePdfReport(reportTitle, headers, data.ToList());
            }
            else if (button == EstoqueReportButton)
            {
                reportTitle = "Relatório de Estoque";
                fileName = "Estoque.pdf";
                var data = await _reportsService.GetEstoqueReportAsync();
                var headers = new[] { "ID", "Produto", "Tipo", "Qtd.", "Data" };
                pdfData = await _pdfReportService.GeneratePdfReport(reportTitle, headers, data.ToList());
            }

            if (pdfData != null)
            {
                // 1. Save the file first and get the path.
                string savedFilePath = await SavePdfToCacheAsync(pdfData, fileName);

                // 2. If the file was saved successfully, navigate to the viewer.
                if (!string.IsNullOrEmpty(savedFilePath))
                {
                    await Navigation.PushAsync(new PdfViewerPage(savedFilePath));
                }
            }
        }
        catch (System.Runtime.InteropServices.COMException comEx)
        {
            // *** PUT A BREAKPOINT ON THE LINE BELOW ***
            // When the breakpoint is hit, hover over 'comEx' to inspect it.
            await DisplayAlert("COM Error", $"HRESULT: {comEx.ErrorCode:X}\nMessage: {comEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro ao gerar o relatório: {ex.Message}", "OK");
        }
        finally
        {
            activityIndicator.IsRunning = false;
            button.IsEnabled = true;
        }
    }

    private async Task<string> SavePdfToCacheAsync(byte[] pdfData, string fileName)
    {
        // Return early if there's no data to save.
        if (pdfData == null || pdfData.Length == 0) return null;

        try
        {
            // 1. Use the cache directory for temporary viewable files.
            string targetDirectory = Path.Combine(FileSystem.CacheDirectory, "PDFs");

            // 2. Ensure the directory exists.
            Directory.CreateDirectory(targetDirectory);

            // 3. (FIX) Make the function more robust by ensuring we only use the filename.
            // This strips any directory information from the incoming 'fileName' parameter,
            // which prevents writing to the wrong location (like the app's 'bin' folder).
            string simpleFileName = Path.GetFileName(fileName);

            // 4. Create a UNIQUE filename using the sanitized filename and a GUID.
            string uniqueFileName = $"{Path.GetFileNameWithoutExtension(simpleFileName)}_{Guid.NewGuid():N}{Path.GetExtension(simpleFileName)}";
            string filePath = Path.Combine(targetDirectory, uniqueFileName);

            // 5. Use a 'using' statement with FileStream to write the data and ensure it's closed.
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await stream.WriteAsync(pdfData, 0, pdfData.Length);
            } // The file stream is automatically closed and disposed of here.

            // On success, return the full path to the newly created unique file.
            return filePath;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Save Error", $"Failed to save the file: {ex.Message}", "OK");
            // On failure, return null.
            return null;
        }
    }
}