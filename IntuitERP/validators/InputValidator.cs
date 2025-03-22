using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntuitERP.models;

namespace IntuitERP.validators
{
    public class InputValidator
    {
        public InputValidatorModel ValidateInputCidade(CidadeModel input)
        {
            var validator = new InputValidatorModel();

            if (input.Cidade == null)
            {
                validator.field.Add("Cidade");
                validator.message.Add("Cidade não pode ser nula");
            }

            if (input.UF == null)
            {
                validator.field.Add("UF");
                validator.message.Add("UF não pode ser nula");
            }

            return validator;
        }

        public InputValidatorModel ValidateInputCliente(ClienteModel input)
        {
            var validator = new InputValidatorModel();

            if (input.Nome == null)
            {
                validator.field.Add("Nome");
                validator.message.Add("Nome não pode ser nulo");
            }

            if (input.CEP == null)
            {
                validator.field.Add("CEP");
                validator.message.Add("CEP não pode ser nulo");
            }

            if (input.CPF == null)
            {
                validator.field.Add("CPF");
                validator.message.Add("CPF não pode ser nulo");
            }

            if (input.Email == null)
            {
                validator.field.Add("Email");
                validator.message.Add("Email não pode ser nulo");
            }

            if (input.DataCadastro == null)
            {
                validator.field.Add("Data do Cadastro");
                validator.message.Add("Data do Cadastro não pode ser nulo");
            }

            if (input.Endereco == null)
            {
                validator.field.Add("Endereco");
                validator.message.Add("Endereco não pode ser nulo");
            }

            if (input.Numero == null)
            {
                validator.field.Add("Numero");
                validator.message.Add("Numero não pode ser nulo");
            }

            if (input.Bairro == null)
            {
                validator.field.Add("Bairro");
                validator.message.Add("Bairro não pode ser nulo");
            }



            return validator;
        }

        public class InputValidatorModel()
        {
            public List<string> field { get; set; }
            public List<string> message { get; set; }
        }
    }
}
