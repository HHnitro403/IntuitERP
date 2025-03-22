using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class CompraModel
    {
        public int CodCompra { get; set; }
        public DateTime? data_compra { get; set; }
        public TimeSpan? hora_compra { get; set; }
        public int? CodFornec { get; set; }
        public decimal? Desconto { get; set; }
        public int? CodVendedor { get; set; }
        public string OBS { get; set; }
        public decimal? valor_total { get; set; }
        public string forma_pagamento { get; set; }
        public bool? status_compra { get; set; }
    }
}
