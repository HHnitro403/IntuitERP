using IntuiERP.Avalonia.UI.models;
using System.Text.RegularExpressions;

namespace IntuiERP.Avalonia.UI.Validators
{
    public class VendedorValidator
    {
        public ModelValidationResult Validate(VendedorModel vendedor)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (string.IsNullOrWhiteSpace(vendedor.NomeVendedor))
            {
                result.AddError("Nome do vendedor � obrigat�rio");
            }
            else if (vendedor.NomeVendedor.Length > 255)
            {
                result.AddError("Nome do vendedor n�o pode exceder 255 caracteres");
            }
            else if (Regex.IsMatch(vendedor.NomeVendedor, @"[^a-zA-Z�-�\s\-\']"))
            {
                result.AddError("Nome do vendedor cont�m caracteres inv�lidos");
            }

            // Stats validation
            if (vendedor.QtdVendas.HasValue && vendedor.QtdVendas < 0)
            {
                result.AddError("Total de vendas no pode ser negativo");
            }

            if (vendedor.QtdVendasFinalizadas.HasValue && vendedor.QtdVendasFinalizadas < 0)
            {
                result.AddError("Vendas finalizadas no pode ser negativo");
            }

            // Check for consistency between totals
            if (vendedor.QtdVendas.HasValue && vendedor.QtdVendasFinalizadas.HasValue)
            {
                if (vendedor.QtdVendasFinalizadas > vendedor.QtdVendas)
                {
                    result.AddError("Vendas finalizadas no pode ser maior que o total de vendas");
                }
            }

            return result;
            }

            // Method to sanitize input
            public VendedorModel Sanitize(VendedorModel vendedor)
            {
            if (vendedor.NomeVendedor != null)
            {
                vendedor.NomeVendedor = vendedor.NomeVendedor.Trim();
            }

            if (vendedor.QtdVendas == null)
            {
                vendedor.QtdVendas = 0;
            }

            if (vendedor.QtdVendasFinalizadas == null)
            {
                vendedor.QtdVendasFinalizadas = 0;
            }

            return vendedor;
            }
    }
}
