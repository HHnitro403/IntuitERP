using IntuitERP.models;
using System.Text.RegularExpressions;

namespace IntuitERP.Validators
{
    public class VendedorValidator
    {
        public ModelValidationResult Validate(VendedorModel vendedor)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (string.IsNullOrWhiteSpace(vendedor.NomeVendedor))
            {
                result.AddError("Nome do vendedor é obrigatório");
            }
            else if (vendedor.NomeVendedor.Length > 255)
            {
                result.AddError("Nome do vendedor não pode exceder 255 caracteres");
            }
            else if (Regex.IsMatch(vendedor.NomeVendedor, @"[^a-zA-ZÀ-ÿ\s\-\']"))
            {
                result.AddError("Nome do vendedor contém caracteres inválidos");
            }

            // Stats validation
            if (vendedor.totalvendas.HasValue && vendedor.totalvendas < 0)
            {
                result.AddError("Total de vendas não pode ser negativo");
            }

            if (vendedor.vendasfinalizadas.HasValue && vendedor.vendasfinalizadas < 0)
            {
                result.AddError("Vendas finalizadas não pode ser negativo");
            }

            if (vendedor.vendascanceladas.HasValue && vendedor.vendascanceladas < 0)
            {
                result.AddError("Vendas canceladas não pode ser negativo");
            }

            // Check for consistency between totals
            if (vendedor.totalvendas.HasValue && vendedor.vendasfinalizadas.HasValue &&
                vendedor.vendascanceladas.HasValue)
            {
                if (vendedor.vendasfinalizadas + vendedor.vendascanceladas > vendedor.totalvendas)
                {
                    result.AddError("A soma de vendas finalizadas e canceladas não pode ser maior que o total de vendas");
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

            if (vendedor.totalvendas == null)
            {
                vendedor.totalvendas = 0;
            }

            if (vendedor.vendasfinalizadas == null)
            {
                vendedor.vendasfinalizadas = 0;
            }

            if (vendedor.vendascanceladas == null)
            {
                vendedor.vendascanceladas = 0;
            }

            return vendedor;
        }
    }
}
