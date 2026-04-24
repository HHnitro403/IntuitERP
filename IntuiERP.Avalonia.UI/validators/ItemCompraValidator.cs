using IntuiERP.Avalonia.UI.models;
using System;

namespace IntuiERP.Avalonia.UI.Validators
{
    public class ItemCompraValidator
    {
        public ModelValidationResult Validate(ItemCompraModel item)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (item.CodCompra <= 0)
            {
                result.AddError("Compra  obrigatria");
            }

            if (item.CodProduto <= 0)
            {
                result.AddError("Produto  obrigatrio");
            }

            if (item.quantidade <= 0)
            {
                result.AddError("Quantidade  obrigatria e deve ser maior que zero");
            }
            else if (item.quantidade > 99999)
            {
                result.AddError("Quantidade no pode exceder 99,999");
            }

            if (item.preco_unitario <= 0)
            {
                result.AddError("Preo unitrio  obrigatrio e deve ser maior que zero");
            }
            else if (item.preco_unitario > 999999.99m)
            {
                result.AddError("Preo unitrio no pode exceder 999,999.99");
            }

            // Subtotal value validation
            if (item.subtotal <= 0)
            {
                result.AddError("Subtotal deve ser maior que zero");
            }
            else
            {
                decimal expectedTotal = item.quantidade * item.preco_unitario;
                if (Math.Abs(item.subtotal - expectedTotal) > 0.01m)
                {
                    result.AddError("Subtotal no corresponde ao clculo: quantidade * preo unitrio");
                }
            }

            return result;
        }

        // Method to sanitize input and calculate values
        public ItemCompraModel Sanitize(ItemCompraModel item)
        {
            // Calculate subtotal
            item.subtotal = item.quantidade * item.preco_unitario;

            return item;
        }
    }
}
