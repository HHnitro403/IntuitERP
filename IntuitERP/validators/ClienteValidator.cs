using IntuitERP.models;
using System;
using System.Text.RegularExpressions;

namespace IntuitERP.Validators
{
    public class ClienteValidator
    {
        public ModelValidationResult Validate(ClienteModel cliente)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (cliente.CodCidade <= 0)
            {
                result.AddError("Cidade é obrigatória");
            }

            if (string.IsNullOrWhiteSpace(cliente.Nome))
            {
                result.AddError("Nome do cliente é obrigatório");
            }
            else if (cliente.Nome.Length > 255)
            {
                result.AddError("Nome do cliente não pode exceder 255 caracteres");
            }
            else if (Regex.IsMatch(cliente.Nome, @"[^a-zA-ZÀ-ÿ\s\-\']"))
            {
                result.AddError("Nome do cliente contém caracteres inválidos");
            }

            // Email validation
            if (!string.IsNullOrWhiteSpace(cliente.Email))
            {
                if (cliente.Email.Length > 255)
                {
                    result.AddError("Email não pode exceder 255 caracteres");
                }
                else if (!IsValidEmail(cliente.Email))
                {
                    result.AddError("Email inválido");
                }
            }

            // Phone validation
            if (!string.IsNullOrWhiteSpace(cliente.Telefone))
            {
                if (cliente.Telefone.Length > 20)
                {
                    result.AddError("Telefone não pode exceder 20 caracteres");
                }
                else if (!IsValidPhone(cliente.Telefone))
                {
                    result.AddError("Telefone inválido");
                }
            }

            // Date of birth validation
            if (cliente.DataNascimento.HasValue)
            {
                if (cliente.DataNascimento.Value > DateTime.Now)
                {
                    result.AddError("Data de nascimento não pode ser no futuro");
                }
                else if (cliente.DataNascimento.Value.Year < 1900)
                {
                    result.AddError("Data de nascimento inválida");
                }
                else if (CalculateAge(cliente.DataNascimento.Value) < 18)
                {
                    result.AddError("Cliente deve ter pelo menos 18 anos");
                }
            }

            // CPF validation
            if (!string.IsNullOrWhiteSpace(cliente.CPF))
            {
                if (cliente.CPF.Length > 14)
                {
                    result.AddError("CPF não pode exceder 14 caracteres");
                }
                else if (!IsValidCPF(cliente.CPF))
                {
                    result.AddError("CPF inválido");
                }
            }

            // Address validation
            if (!string.IsNullOrWhiteSpace(cliente.Endereco) && cliente.Endereco.Length > 255)
            {
                result.AddError("Endereço não pode exceder 255 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Numero) && cliente.Numero.Length > 255)
            {
                result.AddError("Número não pode exceder 255 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Bairro) && cliente.Bairro.Length > 255)
            {
                result.AddError("Bairro não pode exceder 255 caracteres");
            }

            // CEP is required
            if (string.IsNullOrWhiteSpace(cliente.CEP))
            {
                result.AddError("CEP é obrigatório");
            }
            else if (cliente.CEP.Length > 255)
            {
                result.AddError("CEP não pode exceder 255 caracteres");
            }
            else if (!IsValidCEP(cliente.CEP))
            {
                result.AddError("CEP inválido");
            }

            return result;
        }

        // Method to sanitize input
        public ClienteModel Sanitize(ClienteModel cliente)
        {
            if (cliente.Nome != null)
            {
                cliente.Nome = cliente.Nome.Trim();
            }

            if (cliente.Email != null)
            {
                cliente.Email = cliente.Email.Trim().ToLower();
            }

            if (cliente.Telefone != null)
            {
                cliente.Telefone = Regex.Replace(cliente.Telefone, @"[^\d+\-\(\)]", "");
            }

            if (cliente.CPF != null)
            {
                cliente.CPF = Regex.Replace(cliente.CPF, @"[^\d]", "");

                // Format as standard CPF if it has 11 digits
                if (cliente.CPF.Length == 11)
                {
                    cliente.CPF = $"{cliente.CPF.Substring(0, 3)}.{cliente.CPF.Substring(3, 3)}.{cliente.CPF.Substring(6, 3)}-{cliente.CPF.Substring(9, 2)}";
                }
            }

            if (cliente.Endereco != null)
            {
                cliente.Endereco = cliente.Endereco.Trim();
            }

            if (cliente.Numero != null)
            {
                cliente.Numero = cliente.Numero.Trim();
            }

            if (cliente.Bairro != null)
            {
                cliente.Bairro = cliente.Bairro.Trim();
            }

            if (cliente.CEP != null)
            {
                cliente.CEP = Regex.Replace(cliente.CEP, @"[^\d]", "");

                // Format as standard CEP if it has 8 digits
                if (cliente.CEP.Length == 8)
                {
                    cliente.CEP = $"{cliente.CEP.Substring(0, 5)}-{cliente.CEP.Substring(5, 3)}";
                }
            }

            return cliente;
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

        private bool IsValidCPF(string cpf)
        {
            cpf = Regex.Replace(cpf, @"[^\d]", "");

            if (cpf.Length != 11)
                return false;

            // Check if all digits are the same
            bool allDigitsEqual = true;
            for (int i = 1; i < cpf.Length; i++)
            {
                if (cpf[i] != cpf[0])
                {
                    allDigitsEqual = false;
                    break;
                }
            }

            if (allDigitsEqual)
                return false;

            // Calculate first digit
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(cpf[i].ToString()) * (10 - i);

            int remainder = sum % 11;
            int checkDigit1 = remainder < 2 ? 0 : 11 - remainder;

            if (checkDigit1 != int.Parse(cpf[9].ToString()))
                return false;

            // Calculate second digit
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += int.Parse(cpf[i].ToString()) * (11 - i);

            remainder = sum % 11;
            int checkDigit2 = remainder < 2 ? 0 : 11 - remainder;

            return checkDigit2 == int.Parse(cpf[10].ToString());
        }

        private bool IsValidCEP(string cep)
        {
            return Regex.IsMatch(Regex.Replace(cep, @"[^\d]", ""), @"^\d{8}$");
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }
}
