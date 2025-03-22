using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class EstoqueModel
    {
        public int CodEst { get; set; }
        public int CodProd { get; set; }
        public char? Tipo { get; set; }
        public decimal? Qtd { get; set; }
        public DateTime Data { get; set; }
    }
}
