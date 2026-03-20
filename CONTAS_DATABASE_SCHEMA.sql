-- ========================================
-- Contas a Pagar e Contas a Receber
-- Database Schema for IntuitERP
-- ========================================
-- Created: 2025-11-13
-- Purpose: Financial management - Accounts Payable and Receivable with installment support
-- ========================================

-- ========================================
-- CONTAS A RECEBER (Accounts Receivable)
-- ========================================

-- Master table for receivable accounts
CREATE TABLE IF NOT EXISTS `contas_receber` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_venda` INT NOT NULL COMMENT 'Foreign key to vendas table',
  `cod_cliente` INT NOT NULL COMMENT 'Foreign key to clientes table',
  `data_emissao` DATE NOT NULL COMMENT 'Issue date (sale date)',
  `valor_total` DECIMAL(10,2) NOT NULL COMMENT 'Total amount to receive',
  `valor_pago` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Amount already paid',
  `valor_pendente` DECIMAL(10,2) NOT NULL COMMENT 'Remaining balance',
  `num_parcelas` INT NOT NULL DEFAULT 1 COMMENT 'Number of installments',
  `status` ENUM('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado') NOT NULL DEFAULT 'Pendente' COMMENT 'Account status',
  `observacoes` TEXT NULL COMMENT 'Additional notes',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT `fk_contas_receber_venda` FOREIGN KEY (`cod_venda`)
    REFERENCES `vendas`(`CodVenda`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_contas_receber_cliente` FOREIGN KEY (`cod_cliente`)
    REFERENCES `clientes`(`CodCliente`) ON DELETE RESTRICT ON UPDATE CASCADE,

  INDEX `idx_contas_receber_status` (`status`),
  INDEX `idx_contas_receber_cliente` (`cod_cliente`),
  INDEX `idx_contas_receber_data_emissao` (`data_emissao`),
  UNIQUE KEY `uk_contas_receber_venda` (`cod_venda`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Accounts receivable master records';

-- Installments table for receivables
CREATE TABLE IF NOT EXISTS `parcelas_receber` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_conta_receber` INT NOT NULL COMMENT 'Foreign key to contas_receber',
  `numero_parcela` INT NOT NULL COMMENT 'Installment number (1, 2, 3...)',
  `data_vencimento` DATE NOT NULL COMMENT 'Due date',
  `valor_parcela` DECIMAL(10,2) NOT NULL COMMENT 'Installment base amount',
  `valor_pago` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Amount paid for this installment',
  `data_pagamento` DATE NULL COMMENT 'Payment date (null if not paid)',
  `forma_pagamento` VARCHAR(50) NULL COMMENT 'Payment method used',
  `status` ENUM('Pendente', 'Pago', 'Vencido', 'Cancelado') NOT NULL DEFAULT 'Pendente' COMMENT 'Installment status',
  `juros` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Interest/late fees',
  `multa` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Penalty/fine',
  `desconto` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Discount (early payment)',
  `valor_total` DECIMAL(10,2) GENERATED ALWAYS AS (`valor_parcela` + `juros` + `multa` - `desconto`) STORED COMMENT 'Total amount (calculated)',
  `observacoes` TEXT NULL COMMENT 'Additional notes',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT `fk_parcelas_receber_conta` FOREIGN KEY (`cod_conta_receber`)
    REFERENCES `contas_receber`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,

  INDEX `idx_parcelas_receber_status` (`status`),
  INDEX `idx_parcelas_receber_vencimento` (`data_vencimento`),
  UNIQUE KEY `uk_parcelas_receber_conta_num` (`cod_conta_receber`, `numero_parcela`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Receivable installments/parcels';

-- ========================================
-- CONTAS A PAGAR (Accounts Payable)
-- ========================================

-- Master table for payable accounts
CREATE TABLE IF NOT EXISTS `contas_pagar` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_compra` INT NOT NULL COMMENT 'Foreign key to compras table',
  `cod_fornecedor` INT NOT NULL COMMENT 'Foreign key to fornecedor table',
  `data_emissao` DATE NOT NULL COMMENT 'Issue date (purchase date)',
  `valor_total` DECIMAL(10,2) NOT NULL COMMENT 'Total amount to pay',
  `valor_pago` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Amount already paid',
  `valor_pendente` DECIMAL(10,2) NOT NULL COMMENT 'Remaining balance',
  `num_parcelas` INT NOT NULL DEFAULT 1 COMMENT 'Number of installments',
  `status` ENUM('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado') NOT NULL DEFAULT 'Pendente' COMMENT 'Account status',
  `observacoes` TEXT NULL COMMENT 'Additional notes',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT `fk_contas_pagar_compra` FOREIGN KEY (`cod_compra`)
    REFERENCES `compras`(`CodCompra`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_contas_pagar_fornecedor` FOREIGN KEY (`cod_fornecedor`)
    REFERENCES `fornecedor`(`CodFornecedor`) ON DELETE RESTRICT ON UPDATE CASCADE,

  INDEX `idx_contas_pagar_status` (`status`),
  INDEX `idx_contas_pagar_fornecedor` (`cod_fornecedor`),
  INDEX `idx_contas_pagar_data_emissao` (`data_emissao`),
  UNIQUE KEY `uk_contas_pagar_compra` (`cod_compra`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Accounts payable master records';

-- Installments table for payables
CREATE TABLE IF NOT EXISTS `parcelas_pagar` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_conta_pagar` INT NOT NULL COMMENT 'Foreign key to contas_pagar',
  `numero_parcela` INT NOT NULL COMMENT 'Installment number (1, 2, 3...)',
  `data_vencimento` DATE NOT NULL COMMENT 'Due date',
  `valor_parcela` DECIMAL(10,2) NOT NULL COMMENT 'Installment base amount',
  `valor_pago` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Amount paid for this installment',
  `data_pagamento` DATE NULL COMMENT 'Payment date (null if not paid)',
  `forma_pagamento` VARCHAR(50) NULL COMMENT 'Payment method used',
  `status` ENUM('Pendente', 'Pago', 'Vencido', 'Cancelado') NOT NULL DEFAULT 'Pendente' COMMENT 'Installment status',
  `juros` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Interest/late fees',
  `multa` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Penalty/fine',
  `desconto` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT 'Discount (early payment)',
  `valor_total` DECIMAL(10,2) GENERATED ALWAYS AS (`valor_parcela` + `juros` + `multa` - `desconto`) STORED COMMENT 'Total amount (calculated)',
  `observacoes` TEXT NULL COMMENT 'Additional notes',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT `fk_parcelas_pagar_conta` FOREIGN KEY (`cod_conta_pagar`)
    REFERENCES `contas_pagar`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,

  INDEX `idx_parcelas_pagar_status` (`status`),
  INDEX `idx_parcelas_pagar_vencimento` (`data_vencimento`),
  UNIQUE KEY `uk_parcelas_pagar_conta_num` (`cod_conta_pagar`, `numero_parcela`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Payable installments/parcels';

-- ========================================
-- ADDITIONAL SETTINGS
-- ========================================
-- Add financial settings to system_settings table if not exists

INSERT INTO `system_settings` (`setting_key`, `setting_value`, `setting_type`, `category`, `description`, `default_value`) VALUES
('contas_juros_mensal_percent', '1.0', 'decimal', 'financial', 'Monthly interest rate for overdue accounts (%)', '1.0'),
('contas_multa_atraso_percent', '2.0', 'decimal', 'financial', 'Late payment penalty percentage (%)', '2.0'),
('contas_carencia_dias', '0', 'int', 'financial', 'Grace period days before interest applies', '0'),
('contas_intervalo_parcelas_dias', '30', 'int', 'financial', 'Default days between installments', '30'),
('contas_alerta_vencimento_dias', '3', 'int', 'financial', 'Days before due date to show alert', '3')
ON DUPLICATE KEY UPDATE `setting_key`=`setting_key`;

-- ========================================
-- VERIFICATION QUERIES
-- ========================================
-- Run these to verify the schema was created correctly:

-- SELECT COUNT(*) FROM contas_receber;
-- SELECT COUNT(*) FROM parcelas_receber;
-- SELECT COUNT(*) FROM contas_pagar;
-- SELECT COUNT(*) FROM parcelas_pagar;

-- SELECT * FROM system_settings WHERE category = 'financial';

-- ========================================
-- NOTES
-- ========================================
-- Status transitions:
--   Pendente  -> First status when account is created, no payments yet
--   Parcial   -> At least one payment made but not fully paid
--   Pago      -> All installments paid completely
--   Vencido   -> At least one installment is overdue (data_vencimento < CURDATE() and not paid)
--   Cancelado -> Account cancelled (sale/purchase cancelled)
--
-- valor_total calculation for parcelas:
--   valor_total = valor_parcela + juros + multa - desconto (computed column)
--
-- Constraints:
--   - One conta per venda/compra (unique constraint)
--   - Parcelas cascade delete when conta is deleted
--   - Cannot delete venda/compra if conta exists (RESTRICT)
--
-- ========================================
