using IntuitERP.models;
using System;

namespace IntuitERP.Validators
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
                    result.AddError("Data da compra não pode ser no futuro");
                }
                else if (compra.data_compra.Value.Year < 2000)
                {
                    result.AddError("Data da compra é muito antiga");
                }
            }

            // Supplier validation
            if (!compra.CodFornec.HasValue || compra.CodFornec <= 0)
            {
                result.AddError("Fornecedor é obrigatório");
            }

            // Discount validation
            if (compra.Desconto.HasValue && compra.Desconto < 0)
            {
                result.AddError("Desconto não pode ser negativo");
            }
            else if (compra.Desconto.HasValue && compra.valor_total.HasValue &&
                     compra.Desconto > compra.valor_total)
            {
                result.AddError("Desconto não pode ser maior que o valor total");
            }

            // Total value validation
            if (!compra.valor_total.HasValue)
            {
                result.AddError("Valor total é obrigatório");
            }
            else if (compra.valor_total <= 0)
            {
                result.AddError("Valor total deve ser maior que zero");
            }
            else if (compra.valor_total > 9999999.99m)
            {
                result.AddError("Valor total não pode exceder 9,999,999.99");
            }

            // Payment method validation
            if (string.IsNullOrWhiteSpace(compra.forma_pagamento))
            {
                result.AddError("Forma de pagamento é obrigatória");
            }
            else if (compra.forma_pagamento.Length > 50)
            {
                result.AddError("Forma de pagamento não pode exceder 50 caracteres");
            }
            else if (!IsValidPaymentMethod(compra.forma_pagamento))
            {
                result.AddError("Forma de pagamento inválida. Opções: Dinheiro, Cartão de Crédito, Cartão de Débito, Boleto, Transferência, Pix");
            }

            // Purchase status validation
            if (compra.status_compra.HasValue)
            {
                if (compra.status_compra < 0 || compra.status_compra > 3)
                {
                    result.AddError("Status de compra inválido. Valores permitidos: 0 (Pendente), 1 (Confirmada), 2 (Cancelada), 3 (Devolvida)");
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

            if (!compra.hora_compra.HasValue)
            {
                compra.hora_compra = DateTime.Now.TimeOfDay;
            }

            if (compra.forma_pagamento != null)
            {
                compra.forma_pagamento = compra.forma_pagamento.Trim();
            }

            if (compra.OBS != null)
            {
                compra.OBS = compra.OBS.Trim();
            }

            if (!compra.status_compra.HasValue)
            {
                compra.status_compra = 0; // Default: Pending
            }

            return compra;
        }

        // Helper methods
        private bool IsValidPaymentMethod(string method)
        {
            string[] validMethods = new string[]
            {
                "Dinheiro", "Cartão de Crédito", "Cartão de Débito",
                "Boleto", "Transferência", "Pix", "Cheque", "Crediário"
            };

            return Array.Exists(validMethods, m => m.Equals(method, StringComparison.OrdinalIgnoreCase));
        }
    }
}
