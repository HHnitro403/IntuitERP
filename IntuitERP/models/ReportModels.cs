using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.models
{
    public class ReportModels
    {
        public class VendaReportModel
        {
            public int CodVenda { get; set; }
            public DateTime? data_venda { get; set; }
            public string NomeCliente { get; set; }
            public string NomeVendedor { get; set; }
            public decimal? valor_total { get; set; }
            public string forma_pagamento { get; set; }
            public byte? status_venda { get; set; }
        }

        // Model for the Purchases Report
        public class CompraReportModel
        {
            public int CodCompra { get; set; }
            public DateTime? data_compra { get; set; }
            public string NomeFornecedor { get; set; }
            public decimal? valor_total { get; set; }
            public string forma_pagamento { get; set; }
            public byte? status_compra { get; set; }
        }

        // Model for the Products Report
        public class ProdutoReportModel
        {
            public int CodProduto { get; set; }
            public string Descricao { get; set; }
            public string Categoria { get; set; }
            public decimal? PrecoUnitario { get; set; }
            public decimal? SaldoEst { get; set; }
            public string NomeFornecedor { get; set; }
        }

        // Model for the Customers Report
        public class ClienteReportModel
        {
            public int CodCliente { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Telefone { get; set; }
            public string CPF { get; set; }
            public string NomeCidade { get; set; }
            public string UF { get; set; }
        }

        // Model for the Stock Report
        public class EstoqueReportModel
        {
            public int CodEst { get; set; }
            public string DescricaoProduto { get; set; }
            public char Tipo { get; set; }
            public decimal Qtd { get; set; }
            public DateTime Data { get; set; }
        }
    }
}