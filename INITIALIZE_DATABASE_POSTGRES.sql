-- ============================================================================
-- IntuitERP - Unified PostgreSQL Database Initialization Script
-- ============================================================================
-- Purpose: Initialize all core, settings, and financial tables for Supabase/PostgreSQL
-- Version: 1.1.0 (Standardized with Avalonia Services)
-- ============================================================================

-- EXTENSIONS
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. BASE TABLES
-- ============================================================================

-- Cidade (Matches [Table("cidade")])
CREATE TABLE IF NOT EXISTS cidade (
    "CodCIdade" SERIAL PRIMARY KEY,
    "Cidade" VARCHAR(255),
    "UF" VARCHAR(2)
);

-- Usuarios (Matches [Table("usuarios")])
CREATE TABLE IF NOT EXISTS usuarios (
    "CodUsuarios" SERIAL PRIMARY KEY,
    "Usuario" VARCHAR(255) NOT NULL UNIQUE,
    "Senha" VARCHAR(255) NOT NULL,
    "PermissaoProdutosCreate" INT DEFAULT 0,
    "PermissaoProdutosRead" INT DEFAULT 0,
    "PermissaoProdutosUpdate" INT DEFAULT 0,
    "PermissaoProdutosDelete" INT DEFAULT 0,
    "PermissaoVendasCreate" INT DEFAULT 0,
    "PermissaoVendasRead" INT DEFAULT 0,
    "PermissaoVendasUpdate" INT DEFAULT 0,
    "PermissaoVendasDelete" INT DEFAULT 0,
    "PermissaoRelatoriosGenerate" INT DEFAULT 0,
    "PermissaoVendedoresCreate" INT DEFAULT 0,
    "PermissaoVendedoresRead" INT DEFAULT 0,
    "PermissaoVendedoresUpdate" INT DEFAULT 0,
    "PermissaoVendedoresDelete" INT DEFAULT 0,
    "PermissaoFornecedoresCreate" INT DEFAULT 0,
    "PermissaoFornecedoresRead" INT DEFAULT 0,
    "PermissaoFornecedoresUpdate" INT DEFAULT 0,
    "PermissaoFornecedoresDelete" INT DEFAULT 0,
    "PermissaoClientesCreate" INT DEFAULT 0,
    "PermissaoClientesRead" INT DEFAULT 0,
    "PermissaoClientesUpdate" INT DEFAULT 0,
    "PermissaoClientesDelete" INT DEFAULT 0
);

-- Vendedor (Matches [Table("vendedor")])
CREATE TABLE IF NOT EXISTS vendedor (
    "CodVendedor" SERIAL PRIMARY KEY,
    "NomeVendedor" VARCHAR(255) NOT NULL,
    "Comissao" DECIMAL(5,2) DEFAULT 0,
    "Ativo" BOOLEAN DEFAULT TRUE,
    "QtdVendas" INT DEFAULT 0,
    "QtdVendasFinalizadas" INT DEFAULT 0
);

-- Fornecedor (Matches [Table("fornecedor")])
CREATE TABLE IF NOT EXISTS fornecedor (
    "CodFornecedor" SERIAL PRIMARY KEY,
    "razao_social" VARCHAR(255),
    "nome_fantasia" VARCHAR(255),
    "cnpj" VARCHAR(20) UNIQUE,
    "email" VARCHAR(255),
    "telefone" VARCHAR(20),
    "endereco" VARCHAR(255),
    "numero" VARCHAR(50),
    "bairro" VARCHAR(100),
    "CodCidade" INT REFERENCES cidade("CodCIdade"),
    "cep" VARCHAR(20),
    "ativo" BOOLEAN DEFAULT TRUE
);

-- Cliente (Matches [Table("cliente")])
CREATE TABLE IF NOT EXISTS cliente (
    "CodCliente" SERIAL PRIMARY KEY,
    "CodCidade" INT REFERENCES cidade("CodCIdade"),
    "Nome" VARCHAR(255),
    "Email" VARCHAR(255),
    "Telefone" VARCHAR(20),
    "DataNascimento" DATE,
    "CPF" VARCHAR(20),
    "Endereco" VARCHAR(255),
    "Numero" VARCHAR(50),
    "Bairro" VARCHAR(100),
    "CEP" VARCHAR(20),
    "DataCadastro" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "DataUltimaCompra" TIMESTAMP,
    "Ativo" BOOLEAN DEFAULT TRUE
);

-- Produto (Matches [Table("produto")])
CREATE TABLE IF NOT EXISTS produto (
    "CodProduto" SERIAL PRIMARY KEY,
    "Descricao" VARCHAR(255),
    "Categoria" VARCHAR(100),
    "PrecoUnitario" DECIMAL(10,2),
    "SaldoEst" INT DEFAULT 0,
    "FornecedorP_ID" INT REFERENCES fornecedor("CodFornecedor"),
    "DataCadastro" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "EstMinimo" INT DEFAULT 0,
    "EstoqueID" INT,
    "VarianteID" INT,
    "Tipo" VARCHAR(50),
    "Ativo" BOOLEAN DEFAULT TRUE
);

-- Venda (Matches [Table("venda")])
CREATE TABLE IF NOT EXISTS venda (
    "CodVenda" SERIAL PRIMARY KEY,
    "data_venda" DATE DEFAULT CURRENT_DATE,
    "hora_venda" TIME DEFAULT CURRENT_TIME,
    "CodCliente" INT NOT NULL REFERENCES cliente("CodCliente"),
    "Desconto" DECIMAL(10,2) DEFAULT 0,
    "CodVendedor" INT REFERENCES vendedor("CodVendedor"),
    "OBS" TEXT,
    "valor_total" DECIMAL(10,2) NOT NULL,
    "forma_pagamento" VARCHAR(50),
    "status_venda" SMALLINT DEFAULT 1
);

-- Item Venda (Matches [Table("itens_venda")])
CREATE TABLE IF NOT EXISTS itens_venda (
    "ID" SERIAL PRIMARY KEY,
    "CodVenda" INT NOT NULL REFERENCES venda("CodVenda") ON DELETE CASCADE,
    "CodProduto" INT NOT NULL REFERENCES produto("CodProduto"),
    "Quantidade" INT NOT NULL,
    "PrecoUnitario" DECIMAL(10,2) NOT NULL,
    "Descricao" VARCHAR(255),
    "Subtotal" DECIMAL(10,2) GENERATED ALWAYS AS ("Quantidade" * "PrecoUnitario") STORED
);

-- Compra (Matches [Table("compra")])
CREATE TABLE IF NOT EXISTS compra (
    "CodCompra" SERIAL PRIMARY KEY,
    "data_compra" DATE DEFAULT CURRENT_DATE,
    "CodFornecedor" INT NOT NULL REFERENCES fornecedor("CodFornecedor"),
    "valor_total" DECIMAL(10,2) NOT NULL,
    "status_compra" SMALLINT DEFAULT 1
);

-- Item Compra (Matches [Table("itens_compra")])
CREATE TABLE IF NOT EXISTS itens_compra (
    "ID" SERIAL PRIMARY KEY,
    "CodCompra" INT NOT NULL REFERENCES compra("CodCompra") ON DELETE CASCADE,
    "CodProduto" INT NOT NULL REFERENCES produto("CodProduto"),
    "Quantidade" INT NOT NULL,
    "PrecoUnitario" DECIMAL(10,2) NOT NULL,
    "Subtotal" DECIMAL(10,2) GENERATED ALWAYS AS ("Quantidade" * "PrecoUnitario") STORED
);

-- Estoque (Matches [Table("estoque")])
CREATE TABLE IF NOT EXISTS estoque (
    "ID" SERIAL PRIMARY KEY,
    "CodProduto" INT NOT NULL REFERENCES produto("CodProduto"),
    "Tipo" CHAR(1) CHECK ("Tipo" IN ('E', 'S')),
    "Qtd" INT NOT NULL,
    "Data" DATE DEFAULT CURRENT_DATE
);

-- 2. SETTINGS & AUDIT TABLES
-- ============================================================================

-- System Settings
CREATE TABLE IF NOT EXISTS system_settings (
    id SERIAL PRIMARY KEY,
    setting_key VARCHAR(100) NOT NULL UNIQUE,
    setting_value TEXT NOT NULL,
    setting_type VARCHAR(50) NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT,
    default_value TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by INT REFERENCES usuarios("CodUsuarios")
);

-- User Settings
CREATE TABLE IF NOT EXISTS user_settings (
    id SERIAL PRIMARY KEY,
    cod_usuario INT NOT NULL REFERENCES usuarios("CodUsuarios") ON DELETE CASCADE,
    setting_key VARCHAR(100) NOT NULL,
    setting_value TEXT NOT NULL,
    setting_type VARCHAR(50) NOT NULL,
    category VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(cod_usuario, setting_key)
);

-- Settings Audit Log
CREATE TABLE IF NOT EXISTS settings_audit_log (
    id SERIAL PRIMARY KEY,
    setting_type VARCHAR(20) CHECK (setting_type IN ('system', 'user')),
    setting_key VARCHAR(100) NOT NULL,
    old_value TEXT,
    new_value TEXT,
    changed_by INT NOT NULL REFERENCES usuarios("CodUsuarios"),
    changed_for INT REFERENCES usuarios("CodUsuarios"),
    change_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(45),
    user_agent VARCHAR(255)
);

-- 3. FINANCIAL TABLES
-- ============================================================================

-- Contas Receber
CREATE TABLE IF NOT EXISTS contas_receber (
    id SERIAL PRIMARY KEY,
    cod_venda INT NOT NULL UNIQUE REFERENCES venda("CodVenda"),
    cod_cliente INT NOT NULL REFERENCES cliente("CodCliente"),
    data_emissao DATE NOT NULL,
    valor_total DECIMAL(10,2) NOT NULL,
    valor_pago DECIMAL(10,2) DEFAULT 0,
    valor_pendente DECIMAL(10,2) NOT NULL,
    num_parcelas INT DEFAULT 1,
    status VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado')),
    observacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Parcelas Receber
CREATE TABLE IF NOT EXISTS parcelas_receber (
    id SERIAL PRIMARY KEY,
    cod_conta_receber INT NOT NULL REFERENCES contas_receber(id) ON DELETE CASCADE,
    numero_parcela INT NOT NULL,
    data_vencimento DATE NOT NULL,
    valor_parcela DECIMAL(10,2) NOT NULL,
    valor_pago DECIMAL(10,2) DEFAULT 0,
    data_pagamento DATE,
    forma_pagamento VARCHAR(50),
    status VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Pago', 'Vencido', 'Cancelado')),
    juros DECIMAL(10,2) DEFAULT 0,
    multa DECIMAL(10,2) DEFAULT 0,
    desconto DECIMAL(10,2) DEFAULT 0,
    valor_total DECIMAL(10,2) GENERATED ALWAYS AS (valor_parcela + juros + multa - desconto) STORED,
    observacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(cod_conta_receber, numero_parcela)
);

-- Contas Pagar
CREATE TABLE IF NOT EXISTS contas_pagar (
    id SERIAL PRIMARY KEY,
    cod_compra INT NOT NULL UNIQUE REFERENCES compra("CodCompra"),
    cod_fornecedor INT NOT NULL REFERENCES fornecedor("CodFornecedor"),
    data_emissao DATE NOT NULL,
    valor_total DECIMAL(10,2) NOT NULL,
    valor_pago DECIMAL(10,2) DEFAULT 0,
    valor_pendente DECIMAL(10,2) NOT NULL,
    num_parcelas INT DEFAULT 1,
    status VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado')),
    observacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Parcelas Pagar
CREATE TABLE IF NOT EXISTS parcelas_pagar (
    id SERIAL PRIMARY KEY,
    cod_conta_pagar INT NOT NULL REFERENCES contas_pagar(id) ON DELETE CASCADE,
    numero_parcela INT NOT NULL,
    data_vencimento DATE NOT NULL,
    valor_parcela DECIMAL(10,2) NOT NULL,
    valor_pago DECIMAL(10,2) DEFAULT 0,
    data_pagamento DATE,
    forma_pagamento VARCHAR(50),
    status VARCHAR(20) DEFAULT 'Pendente' CHECK (status IN ('Pendente', 'Pago', 'Vencido', 'Cancelado')),
    juros DECIMAL(10,2) DEFAULT 0,
    multa DECIMAL(10,2) DEFAULT 0,
    desconto DECIMAL(10,2) DEFAULT 0,
    valor_total DECIMAL(10,2) GENERATED ALWAYS AS (valor_parcela + juros + multa - desconto) STORED,
    observacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(cod_conta_pagar, numero_parcela)
);

-- 4. INITIAL DATA
-- ============================================================================

-- Default Admin User (Password: admin123)
INSERT INTO usuarios ("Usuario", "Senha", 
    "PermissaoProdutosCreate", "PermissaoProdutosRead", "PermissaoProdutosUpdate", "PermissaoProdutosDelete",
    "PermissaoVendasCreate", "PermissaoVendasRead", "PermissaoVendasUpdate", "PermissaoVendasDelete",
    "PermissaoRelatoriosGenerate",
    "PermissaoVendedoresCreate", "PermissaoVendedoresRead", "PermissaoVendedoresUpdate", "PermissaoVendedoresDelete",
    "PermissaoFornecedoresCreate", "PermissaoFornecedoresRead", "PermissaoFornecedoresUpdate", "PermissaoFornecedoresDelete",
    "PermissaoClientesCreate", "PermissaoClientesRead", "PermissaoClientesUpdate", "PermissaoClientesDelete"
) VALUES (
    'admin', '$2a$12$R9h/lIPzHZ5fJ1B1pI.LReXqE.9K7X/uX7E3j9H7L9G7I7H7I7H7', 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
) ON CONFLICT ("Usuario") DO NOTHING;

-- Default Settings
INSERT INTO system_settings (setting_key, setting_value, setting_type, category, description, default_value) VALUES
('session_timeout_minutes', '30', 'int', 'security', 'Automatic logout after N minutes of inactivity', '30'),
('bcrypt_work_factor', '12', 'int', 'security', 'BCrypt work factor for password hashing', '12'),
('currency_symbol', 'R$', 'string', 'formatting', 'Currency symbol', 'R$'),
('allow_negative_stock', 'false', 'boolean', 'business_rules', 'Allow products to have negative stock balance', 'false')
ON CONFLICT (setting_key) DO NOTHING;

-- Financial Settings
INSERT INTO system_settings (setting_key, setting_value, setting_type, category, description, default_value) VALUES
('contas_juros_mensal_percent', '1.0', 'decimal', 'financial', 'Monthly interest rate for overdue accounts (%)', '1.0'),
('contas_multa_atraso_percent', '2.0', 'decimal', 'financial', 'Late payment penalty percentage (%)', '2.0'),
('contas_intervalo_parcelas_dias', '30', 'int', 'financial', 'Default days between installments', '30')
ON CONFLICT (setting_key) DO NOTHING;
