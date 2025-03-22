using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class ItemVendaModel
    {
        public int CodItem { get; set; }
        public int? CodVenda { get; set; }
        public int? CodProduto { get; set; }
        public string Descricao { get; set; }
        public int? quantidade { get; set; }
        public decimal? valor_unitario { get; set; }
        public decimal? valor_total { get; set; }
        public decimal? desconto { get; set; }
    }
}
