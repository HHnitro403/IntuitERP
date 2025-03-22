using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class ProdutoModel
    {
        public int CodProduto { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public decimal? PrecoUnitario { get; set; }
        public int? SaldoEst { get; set; }
        public int? FornecedorP_ID { get; set; }
        public DateTime? DataCadastro { get; set; }
        public int? EstMinimo { get; set; }
        public int? EstoqueID { get; set; }
        public int? VarianteID { get; set; }
        public string Tipo { get; set; }
        public bool? Ativo { get; set; }
    }
}
