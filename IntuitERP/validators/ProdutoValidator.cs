using IntuitERP.models;
using System;

namespace IntuitERP.Validators
{
    public class ProdutoValidator
    {
        public ModelValidationResult Validate(ProdutoModel produto)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (string.IsNullOrWhiteSpace(produto.Descricao))
            {
                result.AddError("Descrição do produto é obrigatória");
            }
            else if (produto.Descricao.Length > 255)
            {
                result.AddError("Descrição do produto não pode exceder 255 caracteres");
            }

            if (string.IsNullOrWhiteSpace(produto.Categoria))
            {
                result.AddError("Categoria é obrigatória");
            }
            else if (produto.Categoria.Length > 100)
            {
                result.AddError("Categoria não pode exceder 100 caracteres");
            }

            if (!produto.PrecoUnitario.HasValue || produto.PrecoUnitario <= 0)
            {
                result.AddError("Preço unitário é obrigatório e deve ser maior que zero");
            }
            else if (produto.PrecoUnitario > 999999.99m)
            {
                result.AddError("Preço unitário não pode exceder 999,999.99");
            }

            if (produto.EstMinimo.HasValue && produto.EstMinimo < 0)
            {
                result.AddError("Estoque mínimo não pode ser negativo");
            }

            if (produto.SaldoEst.HasValue && produto.SaldoEst < 0)
            {
                result.AddError("Saldo em estoque não pode ser negativo");
            }

            if (produto.FornecedorP_ID <= 0 && produto.FornecedorP_ID.HasValue)
            {
                result.AddError("Fornecedor inválido");
            }

            if (!string.IsNullOrWhiteSpace(produto.Tipo) && produto.Tipo.Length > 50)
            {
                result.AddError("Tipo não pode exceder 50 caracteres");
            }

            return result;
        }

        // Method to sanitize input
        public ProdutoModel Sanitize(ProdutoModel produto)
        {
            if (produto.Descricao != null)
            {
                produto.Descricao = produto.Descricao.Trim();
            }

            if (produto.Categoria != null)
            {
                produto.Categoria = produto.Categoria.Trim();
            }

            if (produto.Tipo != null)
            {
                produto.Tipo = produto.Tipo.Trim();
            }

            if (produto.DataCadastro == null)
            {
                produto.DataCadastro = DateTime.Now;
            }

            if (produto.Ativo == null)
            {
                produto.Ativo = true;
            }

            if (produto.SaldoEst == null)
            {
                produto.SaldoEst = 0;
            }

            if (produto.EstMinimo == null)
            {
                produto.EstMinimo = 0;
            }

            return produto;
        }
    }
}
