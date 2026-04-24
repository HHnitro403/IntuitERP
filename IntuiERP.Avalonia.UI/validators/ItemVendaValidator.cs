using IntuiERP.Avalonia.UI.models;
using System;

namespace IntuiERP.Avalonia.UI.Validators
{
    public class ItemVendaValidator
    {
        public ModelValidationResult Validate(ItemVendaModel item)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (!item.CodVenda.HasValue || item.CodVenda <= 0)
            {
                result.AddError("Venda  obrigatria");
            }

            if (!item.CodProduto.HasValue || item.CodProduto <= 0)
            {
                result.AddError("Produto  obrigatrio");
            }

            if (string.IsNullOrWhiteSpace(item.Descricao))
            {
                result.AddError("Descrio do item  obrigatria");
            }

            if (!item.quantidade.HasValue || item.quantidade <= 0)
            {
                result.AddError("Quantidade  obrigatria e deve ser maior que zero");
            }
            else if (item.quantidade > 99999)
            {
                result.AddError("Quantidade no pode exceder 99,999");
            }

            if (!item.valor_unitario.HasValue || item.valor_unitario <= 0)
            {
                result.AddError("Valor unitrio  obrigatrio e deve ser maior que zero");
            }
            else if (item.valor_unitario > 999999.99m)
            {
                result.AddError("Valor unitrio no pode exceder 999,999.99");
            }

            // Total value validation
            if (item.valor_total.HasValue && item.valor_total <= 0)
            {
                result.AddError("Valor total deve ser maior que zero");
            }
            else if (item.valor_total.HasValue &&
                    item.quantidade.HasValue &&
                    item.valor_unitario.HasValue)
            {
                decimal expectedTotal = item.quantidade.Value * item.valor_unitario.Value;
                if (Math.Abs(item.valor_total.Value - expectedTotal) > 0.01m)
                {
                    result.AddError("Valor total no corresponde ao clculo: quantidade * valor unitrio");
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

            // Calculate total value if not provided or if components have changed
            if (item.quantidade.HasValue && item.valor_unitario.HasValue)
            {
                item.valor_total = item.quantidade.Value * item.valor_unitario.Value;
            }

            return item;
        }
    }
}
