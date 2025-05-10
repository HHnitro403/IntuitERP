using IntuitERP.models;
using System;

namespace IntuitERP.Validators
{
    public class ItemVendaValidator
    {
        public ModelValidationResult Validate(ItemVendaModel item)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (!item.CodVenda.HasValue || item.CodVenda <= 0)
            {
                result.AddError("Venda é obrigatória");
            }

            if (!item.CodProduto.HasValue || item.CodProduto <= 0)
            {
                result.AddError("Produto é obrigatório");
            }

            if (string.IsNullOrWhiteSpace(item.Descricao))
            {
                result.AddError("Descrição do item é obrigatória");
            }

            if (!item.quantidade.HasValue || item.quantidade <= 0)
            {
                result.AddError("Quantidade é obrigatória e deve ser maior que zero");
            }
            else if (item.quantidade > 99999)
            {
                result.AddError("Quantidade não pode exceder 99,999");
            }

            if (!item.valor_unitario.HasValue || item.valor_unitario <= 0)
            {
                result.AddError("Valor unitário é obrigatório e deve ser maior que zero");
            }
            else if (item.valor_unitario > 999999.99m)
            {
                result.AddError("Valor unitário não pode exceder 999,999.99");
            }

            // Discount validation
            if (item.desconto.HasValue && item.desconto < 0)
            {
                result.AddError("Desconto não pode ser negativo");
            }
            else if (item.desconto.HasValue && item.valor_unitario.HasValue &&
                     item.quantidade.HasValue &&
                     item.desconto > (item.valor_unitario * item.quantidade))
            {
                result.AddError("Desconto não pode ser maior que o valor total do item");
            }

            // Total value validation
            if (item.valor_total.HasValue && item.valor_total <= 0)
            {
                result.AddError("Valor total deve ser maior que zero");
            }
            else if (item.valor_total.HasValue &&
                    item.quantidade.HasValue &&
                    item.valor_unitario.HasValue &&
                    item.desconto.HasValue)
            {
                decimal expectedTotal = (item.quantidade.Value * item.valor_unitario.Value) - item.desconto.Value;
                if (Math.Abs(item.valor_total.Value - expectedTotal) > 0.01m)
                {
                    result.AddError("Valor total não corresponde ao cálculo: (quantidade * valor unitário) - desconto");
                }
            }

            return result;
        }

        // Method to sanitize input and calculate values
        public ItemVendaModel Sanitize(ItemVendaModel item)
        {
            if (item.Descricao != null)
            {
                item.Descricao = item.Descricao.Trim();
            }


            if (!item.desconto.HasValue)
            {
                item.desconto = 0;
            }

            // Calculate total value if not provided or if components have changed
            if (item.quantidade.HasValue && item.valor_unitario.HasValue)
            {
                decimal subtotal = item.quantidade.Value * item.valor_unitario.Value;
                item.valor_total = item.desconto.HasValue ? subtotal - item.desconto.Value : subtotal;
            }

            return item;
        }
    }
}
