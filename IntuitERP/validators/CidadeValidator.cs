using IntuitERP.models;
using System.Text.RegularExpressions;

namespace IntuitERP.Validators
{
    public class CidadeValidator
    {
        public ModelValidationResult Validate(CidadeModel cidade)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (string.IsNullOrWhiteSpace(cidade.Cidade))
            {
                result.AddError("Nome da cidade é obrigatório");
            }
            else if (cidade.Cidade.Length > 255)
            {
                result.AddError("Nome da cidade não pode exceder 255 caracteres");
            }
            else if (Regex.IsMatch(cidade.Cidade, @"[^a-zA-ZÀ-ÿ\s\-\']"))
            {
                result.AddError("Nome da cidade contém caracteres inválidos");
            }

            if (string.IsNullOrWhiteSpace(cidade.UF))
            {
                result.AddError("UF é obrigatório");
            }
            else if (cidade.UF.Length != 2)
            {
                result.AddError("UF deve ter exatamente 2 caracteres");
            }
            else if (!Regex.IsMatch(cidade.UF, @"^[A-Z]{2}$"))
            {
                result.AddError("UF deve ser uma sigla válida de estado (ex: SP, RJ)");
            }

            return result;
        }

        // Method to sanitize input
        public CidadeModel Sanitize(CidadeModel cidade)
        {
            if (cidade.Cidade != null)
            {
                cidade.Cidade = cidade.Cidade.Trim();
            }

            if (cidade.UF != null)
            {
                cidade.UF = cidade.UF.Trim().ToUpper();
            }

            return cidade;
        }
    }
}
