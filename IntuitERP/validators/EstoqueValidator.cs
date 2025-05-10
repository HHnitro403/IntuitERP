using IntuitERP.models;
using System;

namespace IntuitERP.Validators
{
    public class EstoqueValidator
    {
        public ModelValidationResult Validate(EstoqueModel estoque)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (estoque.CodProduto <= 0)
            {
                result.AddError("Produto é obrigatório");
            }

            if (!estoque.Tipo.HasValue)
            {
                result.AddError("Tipo de movimentação é obrigatório");
            }
            else if (estoque.Tipo != 'E' && estoque.Tipo != 'S') // Entry or Exit
            {
                result.AddError("Tipo de movimentação deve ser 'E' (Entrada) ou 'S' (Saída)");
            }

            if (!estoque.Qtd.HasValue)
            {
                result.AddError("Quantidade é obrigatória");
            }
            else if (estoque.Qtd <= 0)
            {
                result.AddError("Quantidade deve ser maior que zero");
            }
            else if (estoque.Qtd > 999999.99m)
            {
                result.AddError("Quantidade não pode exceder 999,999.99");
            }

            if (estoque.Data == DateTime.MinValue)
            {
                result.AddError("Data é obrigatória");
            }
            else if (estoque.Data > DateTime.Now)
            {
                result.AddError("Data não pode ser no futuro");
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
