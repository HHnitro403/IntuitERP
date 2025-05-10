using IntuitERP.models;
using System.Text.RegularExpressions;

namespace IntuitERP.Validators
{
    public class UsuarioValidator
    {
        public ModelValidationResult Validate(UsuarioModel usuario)
        {
            var result = new ModelValidationResult();

            // Required fields validation
            if (string.IsNullOrWhiteSpace(usuario.UsuarioNome))
            {
                result.AddError("Nome de usuário é obrigatório");
            }
            else if (usuario.UsuarioNome.Length > 255)
            {
                result.AddError("Nome de usuário não pode exceder 255 caracteres");
            }
            else if (usuario.UsuarioNome.Length < 3)
            {
                result.AddError("Nome de usuário deve ter pelo menos 3 caracteres");
            }
            else if (!Regex.IsMatch(usuario.UsuarioNome, @"^[a-zA-Z0-9_\-\.]+$"))
            {
                result.AddError("Nome de usuário contém caracteres inválidos. Use apenas letras, números, underline, hífen e ponto");
            }

            // Password validation
            if (string.IsNullOrWhiteSpace(usuario.Senha))
            {
                result.AddError("Senha é obrigatória");
            }
            else if (usuario.Senha.Length > 255)
            {
                result.AddError("Senha não pode exceder 255 caracteres");
            }
            else if (usuario.Senha.Length < 6)
            {
                result.AddError("Senha deve ter pelo menos 6 caracteres");
            }
            else if (!HasPasswordComplexity(usuario.Senha))
            {
                result.AddError("Senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial");
            }

            return result;
        }

        // Method to sanitize input
        public UsuarioModel Sanitize(UsuarioModel usuario)
        {
            if (usuario.UsuarioNome != null)
            {
                usuario.UsuarioNome = usuario.UsuarioNome.Trim();
            }

            // Don't sanitize password as it might interfere with special characters needed for security

            return usuario;
        }

        // Helper methods
        private bool HasPasswordComplexity(string password)
        {
            // Check for at least one uppercase letter
            bool hasUppercase = Regex.IsMatch(password, @"[A-Z]");

            // Check for at least one lowercase letter
            bool hasLowercase = Regex.IsMatch(password, @"[a-z]");

            // Check for at least one digit
            bool hasDigit = Regex.IsMatch(password, @"[0-9]");

            // Check for at least one special character
            bool hasSpecialChar = Regex.IsMatch(password, @"[^a-zA-Z0-9]");

            return hasUppercase && hasLowercase && hasDigit && hasSpecialChar;
        }
    }
}
