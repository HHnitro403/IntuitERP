using IntuitERP.models;
using System;

namespace IntuitERP.Validators
{
    public class VendaValidator
    {
        public ModelValidationResult Validate(VendaModel venda)
        {
            var result = new ModelValidationResult();

            // Date validation
            if (venda.data_venda.HasValue)
            {
                if (venda.data_venda.Value > DateTime.Now)
                {
                    result.AddError("Data da venda não pode ser no futuro");
                }
                else if (venda.data_venda.Value.Year < 2000)
                {
                    result.AddError("Data da venda é muito antiga");
                }
            }

            // Client validation
            if (venda.CodCliente <= 0)
            {
                result.AddError("Cliente é obrigatório");
            }

            // Discount validation
            if (venda.Desconto.HasValue && venda.Desconto < 0)
            {
                result.AddError("Desconto não pode ser negativo");
            }
            else if (venda.Desconto.HasValue && venda.Desconto > venda.valor_total)
            {
                result.AddError("Desconto não pode ser maior que o valor total");
            }

            // Total value validation
            if (venda.valor_total <= 0)
            {
                result.AddError("Valor total deve ser maior que zero");
            }
            else if (venda.valor_total > 9999999.99m)
            {
                result.AddError("Valor total não pode exceder 9,999,999.99");
            }

            // Payment method validation
            if (string.IsNullOrWhiteSpace(venda.forma_pagamento))
            {
                result.AddError("Forma de pagamento é obrigatória");
            }
            else if (venda.forma_pagamento.Length > 50)
            {
                result.AddError("Forma de pagamento não pode exceder 50 caracteres");
            }
            else if (!IsValidPaymentMethod(venda.forma_pagamento))
            {
                result.AddError("Forma de pagamento inválida. Opções: Dinheiro, Cartão de Crédito, Cartão de Débito, Boleto, Transferência, Pix");
            }

            // Sale status validation
            if (venda.status_venda.HasValue)
            {
                if (venda.status_venda < 0 || venda.status_venda > 3)
                {
                    result.AddError("Status de venda inválido. Valores permitidos: 0 (Pendente), 1 (Confirmada), 2 (Cancelada), 3 (Devolvida)");
                }
            }

            return result;
        }

        // Method to sanitize input
        public VendaModel Sanitize(VendaModel venda)
        {
            if (!venda.data_venda.HasValue)
            {
                venda.data_venda = DateTime.Now;
            }

            if (!venda.hora_venda.HasValue)
            {
                venda.hora_venda = DateTime.Now.TimeOfDay;
            }

            if (venda.forma_pagamento != null)
            {
                venda.forma_pagamento = venda.forma_pagamento.Trim();
            }

            if (venda.OBS != null)
            {
                venda.OBS = venda.OBS.Trim();
            }

            if (!venda.status_venda.HasValue)
            {
                venda.status_venda = 0; // Default: Pending
            }

            return venda;
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
