using IntuiERP.Avalonia.UI.models;
using System;

namespace IntuiERP.Avalonia.UI.Validators
{
    public class EstoqueValidator
    {
        public ModelValidationResult Validate(EstoqueModel estoque)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (estoque.CodProduto <= 0)
            {
                result.AddError("Produto  obrigatrio");
            }

            if (!estoque.Tipo.HasValue)
            {
                result.AddError("Tipo de movimentao  obrigatrio");
            }
            else if (estoque.Tipo != 'E' && estoque.Tipo != 'S') // Entry or Exit
            {
                result.AddError("Tipo de movimentao deve ser 'E' (Entrada) ou 'S' (Sada)");
            }

            if (estoque.Qtd <= 0)
            {
                result.AddError("Quantidade deve ser maior que zero");
            }
            else if (estoque.Qtd > 999999)
            {
                result.AddError("Quantidade no pode exceder 999,999");
            }

            if (estoque.Data == DateTime.MinValue)
            {
                result.AddError("Data  obrigatria");
            }
            else if (estoque.Data > DateTime.Now)
            {
                result.AddError("Data no pode ser no futuro");
            }

            return result;
        }

        // Method to sanitize input
        public EstoqueModel Sanitize(EstoqueModel estoque)
        {
            if (estoque.Tipo.HasValue)
            {
                // Ensure the type is uppercase
                estoque.Tipo = char.ToUpper(estoque.Tipo.Value);
            }

            if (estoque.Data == DateTime.MinValue)
            {
                estoque.Data = DateTime.Now;
            }

            return estoque;
        }
    }
}
