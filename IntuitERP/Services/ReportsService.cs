using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntuitERP.models.ReportModels;

namespace IntuitERP.Services
{
    public class ReportsService
    {
        private readonly IDbConnection _connection;

        public ReportsService(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Gets data for the general sales report.
        /// Joins venda, cliente, and vendedor tables.
        /// </summary>
        public async Task<IEnumerable<VendaReportModel>> GetVendasReportAsync()
        {
            const string query = @"
                SELECT
                    v.CodVenda,
                    v.data_venda,
                    c.Nome AS NomeCliente,
                    vd.NomeVendedor,
                    v.valor_total,
                    v.forma_pagamento,
                    v.status_venda
                FROM venda v
                INNER JOIN cliente c ON v.CodCliente = c.CodCliente
                INNER JOIN vendedor vd ON v.CodVendedor = vd.CodVendedor
                ORDER BY v.CodVenda DESC;";
            return await _connection.QueryAsync<VendaReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general purchases report.
        /// Joins compra and fornecedor tables.
        /// </summary>
        public async Task<IEnumerable<CompraReportModel>> GetComprasReportAsync()
        {
            const string query = @"
                SELECT
                    c.CodCompra,
                    c.data_compra,
                    f.NomeFantasia AS NomeFornecedor,
                    c.valor_total,
                    c.forma_pagamento,
                    c.status_compra
                FROM compra c
                INNER JOIN fornecedor f ON c.CodFornec = f.CodFornecedor
                ORDER BY c.CodCompra DESC;";
            return await _connection.QueryAsync<CompraReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general products report.
        /// Joins produto and fornecedor tables.
        /// </summary>
        public async Task<IEnumerable<ProdutoReportModel>> GetProdutosReportAsync()
        {
            const string query = @"
                SELECT
                    p.CodProduto,
                    p.Descricao,
                    p.Categoria,
                    p.PrecoUnitario,
                    p.SaldoEst,
                    f.NomeFantasia AS NomeFornecedor
                FROM produto p
                INNER JOIN fornecedor f ON p.FornecedorP_ID = f.CodFornecedor
                ORDER BY p.Descricao;";
            return await _connection.QueryAsync<ProdutoReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general customers report.
        /// Joins cliente and cidade tables.
        /// </summary>
        public async Task<IEnumerable<ClienteReportModel>> GetClientesReportAsync()
        {
            const string query = @"
                SELECT
                    cl.CodCliente,
                    cl.Nome,
                    cl.Email,
                    cl.Telefone,
                    cl.CPF,
                    c.Cidade AS NomeCidade,
                    c.UF
                FROM cliente cl
                INNER JOIN cidade c ON cl.CodCidade = c.CodCIdade
                ORDER BY cl.Nome;";
            return await _connection.QueryAsync<ClienteReportModel>(query);
        }

        /// <summary>
        /// Gets data for the general stock report.
        /// Joins estoque and produto tables.
        /// </summary>
        public async Task<IEnumerable<EstoqueReportModel>> GetEstoqueReportAsync()
        {
            const string query = @"
                SELECT
                    e.CodEst,
                    p.Descricao AS DescricaoProduto,
                    e.Tipo,
                    e.Qtd,
                    e.Data
                FROM estoque e
                INNER JOIN produto p ON e.CodProduto = p.CodProduto
                ORDER BY e.Data DESC;";
            return await _connection.QueryAsync<EstoqueReportModel>(query);
        }
    }
}