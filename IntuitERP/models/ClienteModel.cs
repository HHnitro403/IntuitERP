using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class ClienteModel
    {
        public int CodCliente { get; set; }
        public int CodCidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string CPF { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string CEP { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public DateTime? DataUltimaCompra { get; set; } // Nullable in case no purchase has been made yet
        public bool Ativo { get; set; }
    }
}
