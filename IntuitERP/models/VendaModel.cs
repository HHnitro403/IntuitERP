using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class VendaModel
    {
        public int CodVenda { get; set; }
        public DateTime? data_venda { get; set; }
        public TimeSpan? hora_venda { get; set; }
        public int? CodCliente { get; set; }
        public decimal? Desconto { get; set; }
        public int? CodVendedor { get; set; }
        public string OBS { get; set; }
        public decimal? valor_total { get; set; }
        public string forma_pagamento { get; set; }
        public bool? status_venda { get; set; }
    }
}
