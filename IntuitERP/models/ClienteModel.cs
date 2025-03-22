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
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string CPF { get; set; }
        public string Endereco { get; set; }
        public DateTime? DataCadastro { get; set; }
        public DateTime? DataUltimaCompra { get; set; }
        public bool Ativo { get; set; }
    }
}
