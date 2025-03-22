using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class FornecedorModel
    {
        public int CodFornecedor { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string CNPJ { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; }
        public DateTime? DataCadastro { get; set; }
        public DateTime? DataUltimaCompra { get; set; }
        public bool? Ativo { get; set; }
    }
}
