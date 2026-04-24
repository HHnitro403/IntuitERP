using IntuiERP.Avalonia.UI.models;
using System;

namespace IntuiERP.Avalonia.UI.Validators
{
    public class CompraValidator
    {
        public ModelValidationResult Validate(CompraModel compra)
        {
            var result = new ModelValidationResult();

            // Date validation
            if (compra.data_compra.HasValue)
            {
                if (compra.data_compra.Value > DateTime.Now)
                {
                    result.AddError("Data da compra no pode ser no futuro");
                }
                else if (compra.data_compra.Value.Year < 2000)
                {
                    result.AddError("Data da compra  muito antiga");
                }
            }

            // Supplier validation
            if (!compra.CodFornecedor.HasValue || compra.CodFornecedor <= 0)
            {
                result.AddError("Fornecedor  obrigatrio");
            }

            // Total value validation
            if (!compra.valor_total.HasValue)
            {
                result.AddError("Valor total  obrigatrio");
            }
            else if (compra.valor_total <= 0)
            {
                result.AddError("Valor total deve ser maior que zero");
            }
            else if (compra.valor_total > 9999999.99m)
            {
                result.AddError("Valor total no pode exceder 9,999,999.99");
            }

            // Purchase status validation
            if (compra.status_compra.HasValue)
            {
                if (compra.status_compra < 0 || compra.status_compra > 3)
                {
                    result.AddError("Status de compra invlido. Valores permitidos: 0 (Pendente), 1 (Confirmada), 2 (Cancelada), 3 (Devolvida)");
                }
            }

            return result; 
        }

        // Method to sanitize input
        public CompraModel Sanitize(CompraModel compra)
        {
            if (!compra.data_compra.HasValue)
            {
                compra.data_compra = DateTime.Now;
            }

            if (!compra.status_compra.HasValue)
            {
                compra.status_compra = 1; // Default: Confirmada (based on SQL DEFAULT 1)
            }

            return compra;
        }
    }
}
