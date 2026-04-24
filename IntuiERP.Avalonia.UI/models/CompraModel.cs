using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntuiERP.Avalonia.UI.models
{
    [Table("compra")]
    public class CompraModel
    {
        [Key]
        [Column("cod_compra")]
        public int CodCompra { get; set; }

        [Column("data_compra")]
        public DateTime? data_compra { get; set; }

        [Column("cod_fornecedor")]
        public int? CodFornecedor { get; set; }

        [Column("valor_total")]
        public decimal? valor_total { get; set; }

        [Column("status_compra")]
        public short? status_compra { get; set; }

        // Navigation properties
        [ForeignKey("CodFornecedor")]
        public virtual FornecedorModel? Fornecedor { get; set; }

        public virtual ICollection<ItemCompraModel>? ItensCompra { get; set; }
    }

    public class CompraDisplayModel
    {
        public int CodCompra { get; set; }
        public DateTime? DataCompra { get; set; }
        public decimal ValorTotal { get; set; }
        public string? Status { get; set; }
        public string? NomeFornecedor { get; set; }
        public string? NomeVendedor { get; set; }
    }

    public class CompraFilterModel
    {
        public int? CodFornecedor { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public int? StatusCompra { get; set; }
        // Add any other filters you might need
    }
}
