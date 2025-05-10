using IntuitERP.models;
using System.Text.RegularExpressions;

namespace IntuitERP.Validators
{
    public class FornecedorValidator
    {
        public ModelValidationResult Validate(FornecedorModel fornecedor)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (fornecedor.CodCidade <= 0)
            {
                result.AddError("Cidade é obrigatória");
            }

            if (string.IsNullOrWhiteSpace(fornecedor.RazaoSocial))
            {
                result.AddError("Razão Social é obrigatória");
            }
            else if (fornecedor.RazaoSocial.Length > 255)
            {
                result.AddError("Razão Social não pode exceder 255 caracteres");
            }

            if (string.IsNullOrWhiteSpace(fornecedor.NomeFantasia))
            {
                result.AddError("Nome Fantasia é obrigatório");
            }
            else if (fornecedor.NomeFantasia.Length > 255)
            {
                result.AddError("Nome Fantasia não pode exceder 255 caracteres");
            }

            // CNPJ validation
            if (string.IsNullOrWhiteSpace(fornecedor.CNPJ))
            {
                result.AddError("CNPJ é obrigatório");
            }
            else if (fornecedor.CNPJ.Length > 18)
            {
                result.AddError("CNPJ não pode exceder 18 caracteres");
            }
            else if (!IsValidCNPJ(fornecedor.CNPJ))
            {
                result.AddError("CNPJ inválido");
            }

            // Email validation
            if (!string.IsNullOrWhiteSpace(fornecedor.Email))
            {
                if (fornecedor.Email.Length > 255)
                {
                    result.AddError("Email não pode exceder 255 caracteres");
                }
                else if (!IsValidEmail(fornecedor.Email))
                {
                    result.AddError("Email inválido");
                }
            }

            // Phone validation
            if (!string.IsNullOrWhiteSpace(fornecedor.Telefone))
            {
                if (fornecedor.Telefone.Length > 20)
                {
                    result.AddError("Telefone não pode exceder 20 caracteres");
                }
                else if (!IsValidPhone(fornecedor.Telefone))
                {
                    result.AddError("Telefone inválido");
                }
            }

            // Address validation
            if (!string.IsNullOrWhiteSpace(fornecedor.Endereco) && fornecedor.Endereco.Length > 255)
            {
                result.AddError("Endereço não pode exceder 255 caracteres");
            }

            return result;
        }

        // Method to sanitize input
        public FornecedorModel Sanitize(FornecedorModel fornecedor)
        {
            if (fornecedor.RazaoSocial != null)
            {
                fornecedor.RazaoSocial = fornecedor.RazaoSocial.Trim();
            }

            if (fornecedor.NomeFantasia != null)
            {
                fornecedor.NomeFantasia = fornecedor.NomeFantasia.Trim();
            }

            if (fornecedor.CNPJ != null)
            {
                fornecedor.CNPJ = Regex.Replace(fornecedor.CNPJ, @"[^\d]", "");

                // Format as standard CNPJ if it has 14 digits
                if (fornecedor.CNPJ.Length == 14)
                {
                    fornecedor.CNPJ = $"{fornecedor.CNPJ.Substring(0, 2)}.{fornecedor.CNPJ.Substring(2, 3)}.{fornecedor.CNPJ.Substring(5, 3)}/{fornecedor.CNPJ.Substring(8, 4)}-{fornecedor.CNPJ.Substring(12, 2)}";
                }
            }

            if (fornecedor.Email != null)
            {
                fornecedor.Email = fornecedor.Email.Trim().ToLower();
            }

            if (fornecedor.Telefone != null)
            {
                fornecedor.Telefone = Regex.Replace(fornecedor.Telefone, @"[^\d+\-\(\)]", "");
            }

            if (fornecedor.Endereco != null)
            {
                fornecedor.Endereco = fornecedor.Endereco.Trim();
            }

            return fornecedor;
        }

        // Helper methods
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains("@") && email.Contains(".");
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(Regex.Replace(phone, @"[^\d]", ""), @"^\d{10,11}$");
        }

        private bool IsValidCNPJ(string cnpj)
        {
            cnpj = Regex.Replace(cnpj, @"[^\d]", "");

            if (cnpj.Length != 14)
                return false;

            // Check if all digits are the same
            bool allDigitsEqual = true;
            for (int i = 1; i < cnpj.Length; i++)
            {
                if (cnpj[i] != cnpj[0])
                {
                    allDigitsEqual = false;
                    break;
                }
            }

            if (allDigitsEqual)
                return false;

            // Calculate first digit
            int[] multiplier1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(cnpj[i].ToString()) * multiplier1[i];

            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            if (digit1 != int.Parse(cnpj[12].ToString()))
                return false;

            // Calculate second digit
            int[] multiplier2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += int.Parse(cnpj[i].ToString()) * multiplier2[i];

            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            return digit2 == int.Parse(cnpj[13].ToString());
        }
    }
}
