using IntuitERP.Services;

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
                var pdfPage = new PdfViewerPage(pdfData, fileName);
                await Navigation.PushAsync(pdfPage);
            }
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
}