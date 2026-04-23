-- ========================================
-- Mock Data for IntuitERP (PostgreSQL/Supabase)
-- ========================================

-- Cidades
INSERT INTO cidade ("Cidade", "UF") VALUES
('São Paulo', 'SP'),
('Rio de Janeiro', 'RJ'),
('Belo Horizonte', 'MG'),
('Curitiba', 'PR'),
('Porto Alegre', 'RS'),
('Brasília', 'DF'),
('Salvador', 'BA'),
('Fortaleza', 'CE'),
('Recife', 'PE'),
('Manaus', 'AM');

-- Fornecedor
INSERT INTO fornecedor ("razao_social", "nome_fantasia", "cnpj", "email", "telefone", "endereco", "numero", "bairro", "CodCidade", "cep", "ativo") VALUES
('Tech Distribuidora Ltda', 'TechDist', '12.345.678/0001-90', 'contato@techdist.com.br', '(11) 3456-7890', 'Av. Paulista', '1000', 'Bela Vista', 1, '01310-100', TRUE),
('Eletrônica Brasil S.A.', 'EletronicaBR', '23.456.789/0001-01', 'vendas@eletronicabr.com.br', '(21) 2345-6789', 'Rua da Assembleia', '10', 'Centro', 2, '20011-000', TRUE),
('Mega Periféricos Importadora', 'MegaPeriferico', '34.567.890/0001-12', 'comercial@megaperifericos.com.br', '(31) 3234-5678', 'Av. Afonso Pena', '500', 'Centro', 3, '30130-001', TRUE),
('InfoParts Distribuidora', 'InfoParts', '45.678.901/0001-23', 'info@infoparts.com.br', '(41) 3123-4567', 'Rua XV de Novembro', '200', 'Centro', 4, '80020-310', TRUE),
('Global Tech Comércio', 'GlobalTech', '56.789.012/0001-34', 'global@globaltech.com.br', '(51) 3012-3456', 'Av. Borges de Medeiros', '300', 'Centro Histórico', 5, '90020-021', TRUE);

-- Produto
INSERT INTO produto ("Descricao", "Categoria", "PrecoUnitario", "SaldoEst", "EstMinimo", "Tipo", "FornecedorP_ID", "Ativo") VALUES
('Notebook Dell Inspiron 15', 'Notebooks', 3299.00, 25, 5, 'Revenda', 1, TRUE),
('Notebook Lenovo IdeaPad 3', 'Notebooks', 2899.00, 18, 5, 'Revenda', 1, TRUE),
('Notebook Acer Aspire 5', 'Notebooks', 4599.00, 12, 3, 'Revenda', 2, TRUE),
('MacBook Air M2', 'Notebooks', 9999.00, 5, 2, 'Revenda', 2, TRUE),
('Mouse Logitech MX Master 3', 'Periféricos', 499.00, 120, 20, 'Revenda', 3, TRUE),
('Mouse Gamer Razer DeathAdder', 'Periféricos', 179.00, 85, 15, 'Revenda', 3, TRUE),
('Teclado Mecânico Keychron K2', 'Periféricos', 699.00, 45, 10, 'Revenda', 3, TRUE),
('Monitor LG 27pol 4K', 'Monitores', 1899.00, 15, 3, 'Monitores', 2, TRUE),
('Cadeira Gamer DXRacer', 'Móveis', 1499.00, 8, 2, 'Móveis', 4, TRUE),
('SSD Kingston 1TB', 'Componentes', 449.00, 65, 15, 'Componentes', 5, TRUE);

-- Cliente
INSERT INTO cliente ("Nome", "Email", "Telefone", "CPF", "DataNascimento", "Endereco", "Numero", "Bairro", "CodCidade", "CEP", "Ativo") VALUES
('TechCorp Soluções Ltda', 'compras@techcorp.com.br', '(11) 98765-4321', '12.345.678/0001-90', '2010-03-15', 'Av. Paulista', '1000', 'Bela Vista', 1, '01310-100', TRUE),
('InnovaNet Tecnologia', 'contato@innovanet.com.br', '(21) 97654-3210', '23.456.789/0001-01', '2012-07-20', 'Rua da Assembleia', '10', 'Centro', 2, '20011-000', TRUE),
('João Pedro Silva', 'joao.silva@email.com', '(11) 94321-0987', '123.456.789-00', '1990-05-15', 'Rua das Flores', '123', 'Jardim Paulista', 1, '01234-000', TRUE),
('Maria Santos Oliveira', 'maria.santos@email.com', '(21) 93210-9876', '234.567.890-11', '1985-08-22', 'Av. Atlântica', '456', 'Copacabana', 2, '22070-001', TRUE);

-- Vendedor
INSERT INTO vendedor ("NomeVendedor", "QtdVendas", "QtdVendasFinalizadas", "Comissao", "Ativo") VALUES
('João Silva Santos', 145, 132, 5.0, TRUE),
('Maria Oliveira Costa', 198, 185, 5.0, TRUE),
('Pedro Henrique Souza', 123, 115, 5.0, TRUE);

-- Venda
INSERT INTO venda ("data_venda", "hora_venda", "valor_total", "forma_pagamento", "status_venda", "OBS", "CodCliente", "CodVendedor") VALUES
('2025-11-10', '10:30:00', 4548.00, 'PIX', 2, 'Cliente corporativo', 1, 2),
('2025-11-11', '14:15:00', 1899.00, 'Cartão de Crédito', 2, NULL, 4, 1);

-- Itens Venda
INSERT INTO itens_venda ("CodProduto", "CodVenda", "Quantidade", "PrecoUnitario", "Descricao") VALUES
(5, 1, 10, 499.00, 'Mouse Logitech MX Master 3'),
(7, 1, 5, 699.00, 'Teclado Mecânico Keychron K2'),
(8, 2, 1, 1899.00, 'Monitor LG 27pol 4K');

-- Compra
INSERT INTO compra ("data_compra", "valor_total", "status_compra", "CodFornecedor") VALUES
('2025-11-01', 82475.00, 2, 1);

-- Itens Compra
INSERT INTO itens_compra ("CodProduto", "CodCompra", "Quantidade", "PrecoUnitario") VALUES
(1, 1, 25, 3299.00);

-- Estoque
INSERT INTO estoque ("CodProduto", "Tipo", "Qtd", "Data") VALUES
(1, 'E', 25, '2025-11-01'),
(5, 'S', 10, '2025-11-10');
