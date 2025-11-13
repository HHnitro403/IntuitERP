# Implementation Plan: Contas a Pagar & Contas a Receber

**Date**: 2025-11-13
**Status**: 🔍 PENDING APPROVAL
**Priority**: 🔴 HIGH (Cornerstone Feature)

---

## 📋 Overview

This plan outlines the implementation of two critical financial management modules:

1. **Contas a Receber** (Accounts Receivable) - Money owed TO the business BY customers
2. **Contas a Pagar** (Accounts Payable) - Money owed BY the business TO suppliers

Both modules will support **installment payments** and integrate seamlessly with existing Sales and Purchases modules.

---

## 🗄️ Database Schema

### Table 1: `contas_receber` (Receivables Master)

```sql
CREATE TABLE `contas_receber` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_venda` INT NOT NULL,
  `cod_cliente` INT NOT NULL,
  `data_emissao` DATE NOT NULL COMMENT 'Issue date (sale date)',
  `valor_total` DECIMAL(10,2) NOT NULL COMMENT 'Total amount to receive',
  `valor_pago` DECIMAL(10,2) DEFAULT 0 COMMENT 'Amount already paid',
  `valor_pendente` DECIMAL(10,2) NOT NULL COMMENT 'Remaining balance',
  `num_parcelas` INT NOT NULL DEFAULT 1 COMMENT 'Number of installments',
  `status` ENUM('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado') DEFAULT 'Pendente',
  `observacoes` TEXT,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (`cod_venda`) REFERENCES `vendas`(`CodVenda`) ON DELETE RESTRICT,
  FOREIGN KEY (`cod_cliente`) REFERENCES `clientes`(`CodCliente`) ON DELETE RESTRICT,
  INDEX `idx_status` (`status`),
  INDEX `idx_cliente` (`cod_cliente`),
  INDEX `idx_data_emissao` (`data_emissao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Accounts receivable master records';
```

**Status Logic**:
- `Pendente` = No payments made yet (valor_pago = 0)
- `Parcial` = Partially paid (0 < valor_pago < valor_total)
- `Pago` = Fully paid (valor_pago >= valor_total)
- `Vencido` = Has at least one overdue installment
- `Cancelado` = Account cancelled (sale cancelled)

### Table 2: `parcelas_receber` (Receivable Installments)

```sql
CREATE TABLE `parcelas_receber` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_conta_receber` INT NOT NULL,
  `numero_parcela` INT NOT NULL COMMENT 'Installment number (1, 2, 3...)',
  `data_vencimento` DATE NOT NULL COMMENT 'Due date',
  `valor_parcela` DECIMAL(10,2) NOT NULL COMMENT 'Installment base amount',
  `valor_pago` DECIMAL(10,2) DEFAULT 0 COMMENT 'Amount paid for this installment',
  `data_pagamento` DATE NULL COMMENT 'Payment date',
  `forma_pagamento` VARCHAR(50) NULL COMMENT 'Payment method used',
  `status` ENUM('Pendente', 'Pago', 'Vencido', 'Cancelado') DEFAULT 'Pendente',
  `juros` DECIMAL(10,2) DEFAULT 0 COMMENT 'Interest/late fees',
  `multa` DECIMAL(10,2) DEFAULT 0 COMMENT 'Penalty/fine',
  `desconto` DECIMAL(10,2) DEFAULT 0 COMMENT 'Discount (early payment)',
  `valor_total` DECIMAL(10,2) GENERATED ALWAYS AS (valor_parcela + juros + multa - desconto) STORED,
  `observacoes` TEXT,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (`cod_conta_receber`) REFERENCES `contas_receber`(`id`) ON DELETE CASCADE,
  INDEX `idx_status` (`status`),
  INDEX `idx_vencimento` (`data_vencimento`),
  UNIQUE KEY `uk_conta_parcela` (`cod_conta_receber`, `numero_parcela`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Receivable installments/parcels';
```

**Status Logic**:
- `Pendente` = Not paid and not past due date
- `Vencido` = Not paid and past due date (CURDATE() > data_vencimento)
- `Pago` = Fully paid (valor_pago >= valor_total)
- `Cancelado` = Installment cancelled

### Table 3: `contas_pagar` (Payables Master)

```sql
CREATE TABLE `contas_pagar` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_compra` INT NOT NULL,
  `cod_fornecedor` INT NOT NULL,
  `data_emissao` DATE NOT NULL COMMENT 'Issue date (purchase date)',
  `valor_total` DECIMAL(10,2) NOT NULL COMMENT 'Total amount to pay',
  `valor_pago` DECIMAL(10,2) DEFAULT 0 COMMENT 'Amount already paid',
  `valor_pendente` DECIMAL(10,2) NOT NULL COMMENT 'Remaining balance',
  `num_parcelas` INT NOT NULL DEFAULT 1 COMMENT 'Number of installments',
  `status` ENUM('Pendente', 'Parcial', 'Pago', 'Vencido', 'Cancelado') DEFAULT 'Pendente',
  `observacoes` TEXT,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (`cod_compra`) REFERENCES `compras`(`CodCompra`) ON DELETE RESTRICT,
  FOREIGN KEY (`cod_fornecedor`) REFERENCES `fornecedor`(`CodFornecedor`) ON DELETE RESTRICT,
  INDEX `idx_status` (`status`),
  INDEX `idx_fornecedor` (`cod_fornecedor`),
  INDEX `idx_data_emissao` (`data_emissao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Accounts payable master records';
```

### Table 4: `parcelas_pagar` (Payable Installments)

```sql
CREATE TABLE `parcelas_pagar` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `cod_conta_pagar` INT NOT NULL,
  `numero_parcela` INT NOT NULL COMMENT 'Installment number (1, 2, 3...)',
  `data_vencimento` DATE NOT NULL COMMENT 'Due date',
  `valor_parcela` DECIMAL(10,2) NOT NULL COMMENT 'Installment base amount',
  `valor_pago` DECIMAL(10,2) DEFAULT 0 COMMENT 'Amount paid for this installment',
  `data_pagamento` DATE NULL COMMENT 'Payment date',
  `forma_pagamento` VARCHAR(50) NULL COMMENT 'Payment method used',
  `status` ENUM('Pendente', 'Pago', 'Vencido', 'Cancelado') DEFAULT 'Pendente',
  `juros` DECIMAL(10,2) DEFAULT 0 COMMENT 'Interest/late fees',
  `multa` DECIMAL(10,2) DEFAULT 0 COMMENT 'Penalty/fine',
  `desconto` DECIMAL(10,2) DEFAULT 0 COMMENT 'Discount (early payment)',
  `valor_total` DECIMAL(10,2) GENERATED ALWAYS AS (valor_parcela + juros + multa - desconto) STORED,
  `observacoes` TEXT,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (`cod_conta_pagar`) REFERENCES `contas_pagar`(`id`) ON DELETE CASCADE,
  INDEX `idx_status` (`status`),
  INDEX `idx_vencimento` (`data_vencimento`),
  UNIQUE KEY `uk_conta_parcela` (`cod_conta_pagar`, `numero_parcela`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Payable installments/parcels';
```

---

## 🏗️ Architecture Components

### Models (8 files)

1. **IntuitERP/models/ContaReceberModel.cs**
   ```csharp
   - Properties: Id, CodVenda, CodCliente, DataEmissao, ValorTotal, ValorPago, ValorPendente, NumParcelas, Status, Observacoes
   - Navigation: Cliente (ClienteModel), Venda (VendaModel)
   - Calculated: PercentualPago, DiasVencido (for overdue)
   ```

2. **IntuitERP/models/ParcelaReceberModel.cs**
   ```csharp
   - Properties: Id, CodContaReceber, NumeroParcela, DataVencimento, ValorParcela, ValorPago, DataPagamento, FormaPagamento, Status, Juros, Multa, Desconto, ValorTotal
   - Calculated: IsVencida (past due), DiasAtraso, ValorRestante
   ```

3. **IntuitERP/models/ContaPagarModel.cs**
   ```csharp
   - Properties: Id, CodCompra, CodFornecedor, DataEmissao, ValorTotal, ValorPago, ValorPendente, NumParcelas, Status, Observacoes
   - Navigation: Fornecedor (FornecedorModel), Compra (CompraModel)
   - Calculated: PercentualPago, DiasVencido
   ```

4. **IntuitERP/models/ParcelaPagarModel.cs**
   ```csharp
   - Properties: Id, CodContaPagar, NumeroParcela, DataVencimento, ValorParcela, ValorPago, DataPagamento, FormaPagamento, Status, Juros, Multa, Desconto, ValorTotal
   - Calculated: IsVencida, DiasAtraso, ValorRestante
   ```

5-8. **Display Models** (for DataGrid):
   - `ContaReceberDisplayModel` (with cliente name, status color)
   - `ParcelaReceberDisplayModel` (with formatted dates, status color)
   - `ContaPagarDisplayModel` (with fornecedor name, status color)
   - `ParcelaPagarDisplayModel` (with formatted dates, status color)

### Services (4 files)

1. **IntuitERP/Services/ContaReceberService.cs**
   - `GetAllAsync()` - List all receivables
   - `GetByIdAsync(int id)` - Get by ID
   - `GetByVendaAsync(int codVenda)` - Get by sale
   - `GetByClienteAsync(int codCliente)` - Get by customer
   - `GetByStatusAsync(string status)` - Filter by status
   - `GetVencidasAsync()` - Get overdue accounts
   - `GetVencendoHojeAsync()` - Get accounts due today
   - `InsertAsync(ContaReceberModel conta)` - Create account
   - `UpdateAsync(ContaReceberModel conta)` - Update account
   - `DeleteAsync(int id)` - Delete account (cascade to parcelas)
   - `RecalcularStatusAsync(int id)` - Recalculate status based on parcelas
   - `GetDashboardSummaryAsync()` - Summary for dashboard

2. **IntuitERP/Services/ParcelaReceberService.cs**
   - `GetByContaAsync(int codConta)` - Get all parcelas for account
   - `GetByIdAsync(int id)` - Get by ID
   - `RegistrarPagamentoAsync(int id, decimal valor, DateTime dataPagamento, string formaPagamento)` - Register payment
   - `RegistrarPagamentoParcialAsync(...)` - Register partial payment
   - `CancelarParcelaAsync(int id)` - Cancel installment
   - `AtualizarStatusVencimentoAsync()` - Update overdue status (batch job)
   - `CalcularJurosMultaAsync(int id)` - Calculate interest/penalties

3. **IntuitERP/Services/ContaPagarService.cs**
   - Same methods as ContaReceberService but for payables

4. **IntuitERP/Services/ParcelaPagarService.cs**
   - Same methods as ParcelaReceberService but for payables

### Views (8 files)

#### Contas a Receber Views

1. **IntuitERP/Viwes/Search/ContasReceberSearch.xaml[.cs]**
   - **Layout**:
     ```
     ┌─────────────────────────────────────────────────┐
     │ Contas a Receber                    [Nova Conta]│
     ├─────────────────────────────────────────────────┤
     │ Dashboard Cards:                                │
     │ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐│
     │ │Pendente │ │ Vencido │ │ Parcial │ │  Pago   ││
     │ │R$ X,XXX │ │R$ X,XXX │ │R$ X,XXX │ │R$ X,XXX ││
     │ └─────────┘ └─────────┘ └─────────┘ └─────────┘│
     ├─────────────────────────────────────────────────┤
     │ Filtros:                                        │
     │ Status: [Todos ▼] Cliente: [Buscar...        ] │
     │ Período: [01/11/2025] a [30/11/2025] [Filtrar]│
     ├─────────────────────────────────────────────────┤
     │ DataGrid:                                       │
     │ ID | Cliente | Venda | Emissão | Valor | Pago |│
     │ XX | Nome    | #XXX  | DD/MM   | R$ XX | XX%  |│
     │ ... (Status-colored rows)                       │
     ├─────────────────────────────────────────────────┤
     │ [Ver Parcelas] [Registrar Pagamento] [Excluir] │
     └─────────────────────────────────────────────────┘
     ```

   - **Features**:
     - Summary cards with totals by status
     - Color-coded DataGrid rows (🟢 Pago, 🟡 Parcial, 🔴 Vencido, ⚪ Pendente)
     - Filter by status, customer, date range
     - Quick actions: View installments, Register payment, Delete
     - Double-click row to view/edit details
     - Search bar for customer name

2. **IntuitERP/Viwes/CadastroContaReceber.xaml[.cs]**
   - **Layout**:
     ```
     ┌─────────────────────────────────────────────────┐
     │ Conta a Receber - [Nova/Editar]                │
     ├─────────────────────────────────────────────────┤
     │ Informações Gerais:                             │
     │ Venda: [#XXX - Cliente: Nome do Cliente      ] │
     │ Data Emissão: [13/11/2025]                      │
     │ Valor Total: [R$ 1.000,00]                      │
     ├─────────────────────────────────────────────────┤
     │ Parcelamento:                                   │
     │ Número de Parcelas: [1▼] [Gerar Parcelas]      │
     │ Primeira Parcela: [13/12/2025]                  │
     │ Intervalo (dias): [30]                          │
     ├─────────────────────────────────────────────────┤
     │ Parcelas:                                       │
     │ DataGrid (editable):                            │
     │ # | Vencimento | Valor     | Status | Ações    │
     │ 1 | 13/12/2025 | R$ 333,33 | Pend.  | [Pagar]  │
     │ 2 | 13/01/2026 | R$ 333,33 | Pend.  | [Pagar]  │
     │ 3 | 13/02/2026 | R$ 333,34 | Pend.  | [Pagar]  │
     ├─────────────────────────────────────────────────┤
     │ Observações:                                    │
     │ [                                             ] │
     ├─────────────────────────────────────────────────┤
     │              [Salvar]  [Cancelar]               │
     └─────────────────────────────────────────────────┘
     ```

   - **Features**:
     - Auto-populate from sale (cliente, valor, data)
     - Installment generator (equal or custom)
     - Editable installment grid
     - Payment registration modal
     - Validation (total must match sale value)

3. **IntuitERP/Viwes/RegistrarPagamentoParcela.xaml[.cs]** (Modal)
   - **Layout**:
     ```
     ┌──────────────────────────────────────┐
     │ Registrar Pagamento - Parcela #X     │
     ├──────────────────────────────────────┤
     │ Valor Original: R$ 333,33            │
     │ Juros/Multa:   [R$ 0,00]             │
     │ Desconto:      [R$ 0,00]             │
     │ ─────────────────────────────────    │
     │ Valor Total:    R$ 333,33            │
     ├──────────────────────────────────────┤
     │ Valor Pago:    [R$ 333,33         ]  │
     │ Data Pagamento:[13/11/2025        ]  │
     │ Forma Pgto:    [PIX              ▼]  │
     ├──────────────────────────────────────┤
     │ Observações:                         │
     │ [                                  ] │
     ├──────────────────────────────────────┤
     │        [Confirmar]  [Cancelar]       │
     └──────────────────────────────────────┘
     ```

   - **Features**:
     - Auto-calculate total with interest/penalties/discount
     - Support partial payments
     - Payment method from settings (GetPaymentMethodsAsync)
     - Date picker with default = today
     - Update both parcela and conta status

#### Contas a Pagar Views

4. **IntuitERP/Viwes/Search/ContasPagarSearch.xaml[.cs]**
   - Identical structure to ContasReceberSearch
   - Shows Fornecedor instead of Cliente
   - Shows Compra instead of Venda

5. **IntuitERP/Viwes/CadastroContaPagar.xaml[.cs]**
   - Identical structure to CadastroContaReceber
   - Auto-populate from purchase

6. **IntuitERP/Viwes/RegistrarPagamentoParcelaPagar.xaml[.cs]** (Modal)
   - Identical to RegistrarPagamentoParcela but for payables

---

## 🔄 Business Logic & Rules

### Account Creation

**Contas a Receber**:
1. Can only be created from FATURADA sales (status = 2)
2. One conta_receber per venda (1:1 relationship)
3. If venda already has conta_receber, show error
4. Auto-populate: cod_venda, cod_cliente, valor_total, data_emissao

**Contas a Pagar**:
1. Can only be created from CONCLUÍDA purchases (status = 2)
2. One conta_pagar per compra (1:1 relationship)
3. If compra already has conta_pagar, show error
4. Auto-populate: cod_compra, cod_fornecedor, valor_total, data_emissao

### Installment Generation

1. **Equal Installments**:
   - User enters: num_parcelas, primeira_vencimento, intervalo_dias
   - System calculates: valor_parcela = valor_total / num_parcelas
   - Last installment gets remainder (e.g., 1000 / 3 = 333.33, 333.33, 333.34)
   - Generate due dates: first + (intervalo * parcela_num)

2. **Custom Installments**:
   - User can manually edit each parcela value and due date
   - Validation: SUM(valores_parcelas) must equal valor_total

3. **Single Payment** (num_parcelas = 1):
   - Create one parcela with full valor_total
   - Due date = data_emissao + default_days (e.g., 30)

### Status Management

**Auto-update triggers**:

1. **Parcela Status** (updated on payment or daily cron):
   ```
   IF valor_pago >= valor_total THEN
       status = 'Pago'
   ELSE IF CURDATE() > data_vencimento THEN
       status = 'Vencido'
   ELSE
       status = 'Pendente'
   END IF
   ```

2. **Conta Status** (updated after parcela changes):
   ```
   parcelas = GetAllParcelas(conta_id)
   valor_pago_total = SUM(parcelas.valor_pago)

   UPDATE conta SET valor_pago = valor_pago_total
   UPDATE conta SET valor_pendente = valor_total - valor_pago_total

   IF valor_pago_total >= valor_total THEN
       status = 'Pago'
   ELSE IF valor_pago_total > 0 THEN
       status = 'Parcial'
   ELSE IF EXISTS(parcela WHERE status = 'Vencido') THEN
       status = 'Vencido'
   ELSE
       status = 'Pendente'
   END IF
   ```

### Payment Registration

**Process Flow**:
1. User selects parcela and clicks "Registrar Pagamento"
2. Modal opens with parcela details
3. User can add juros/multa (late fees) or desconto (early payment discount)
4. System calculates valor_total = valor_parcela + juros + multa - desconto
5. User enters valor_pago (can be partial or full)
6. User selects forma_pagamento from settings
7. On save:
   - Update parcela: valor_pago, data_pagamento, forma_pagamento, status
   - Recalculate conta: valor_pago, valor_pendente, status
   - Transaction ensures atomicity

**Partial Payment Support**:
- If valor_pago < valor_total, parcela remains 'Pendente' or 'Vencido'
- Allow multiple payment registrations until fully paid
- Track payment history (could add parcela_pagamentos table in future)

### Interest & Penalty Calculation

**Configuration** (via Settings System):
```csharp
- juros_mensal_percent = 1.0% (from settings)
- multa_atraso_percent = 2.0% (from settings)
- carencia_dias = 0 (grace period)
```

**Calculation** (on payment modal open):
```csharp
IF data_pagamento > data_vencimento + carencia_dias THEN
    dias_atraso = (data_pagamento - data_vencimento - carencia_dias).Days

    // Multa (one-time penalty)
    multa = valor_parcela * (multa_atraso_percent / 100)

    // Juros (pro-rata interest)
    juros_diario = (juros_mensal_percent / 30) / 100
    juros = valor_parcela * juros_diario * dias_atraso

    valor_total = valor_parcela + juros + multa
END IF
```

### Cancellation

**Cancel Conta**:
1. Set conta.status = 'Cancelado'
2. Set all parcelas.status = 'Cancelado'
3. Cannot cancel if any parcela is already paid (show error)
4. Update venda/compra to reflect cancellation

**Cancel Individual Parcela**:
1. Can only cancel if not paid
2. Recalculate conta: reduce valor_total, update num_parcelas
3. Transaction ensures consistency

---

## 🎨 UI/UX Design (Matching Existing Screens)

### Color Scheme (Status Indicators)

```csharp
// Status colors (matching MAUI theme)
Pendente  = Colors.Gray     (⚪ #808080)
Parcial   = Colors.Orange   (🟠 #FFA500)
Pago      = Colors.Green    (🟢 #28A745)
Vencido   = Colors.Red      (🔴 #DC3545)
Cancelado = Colors.DarkGray (⚫ #555555)
```

### DataGrid Styling

- Alternating row colors for readability
- Status-colored left border (5px)
- Bold text for overdue amounts
- Icon indicators: ✓ (paid), ⚠ (overdue), ⏳ (pending)

### Dashboard Cards

```
┌──────────────────┐
│   R$ 12.500,00   │  ← Large, bold
│   ────────────   │
│     Pendente     │  ← Status label
│   10 contas      │  ← Count
└──────────────────┘
```

### Navigation Integration

**MaenuPage.xaml** additions:
```xml
<Button Text="Contas a Receber" Clicked="ContasReceberButton_Clicked" />
<Button Text="Contas a Pagar" Clicked="ContasPagarButton_Clicked" />
```

**VendaSearch.xaml** additions:
- Button: "Gerar Conta a Receber" (enabled only if status = Faturada)
- Show icon if conta already exists

**CompraSearch.xaml** additions:
- Button: "Gerar Conta a Pagar" (enabled only if status = Concluída)
- Show icon if conta already exists

---

## 🔌 Integration Points

### 1. Venda ↔ Conta Receber

**VendaSearch.xaml**:
- Add column: "Conta Receber" with status badge
- Add button: "Gerar Conta" (creates conta from selected venda)
- On click: Navigate to CadastroContaReceber with venda pre-filled

**CadastrodeVenda.xaml**:
- Show warning if venda is faturada but has no conta_receber
- Add link: "Criar Conta a Receber"

### 2. Compra ↔ Conta Pagar

**CompraSearch.xaml**:
- Add column: "Conta Pagar" with status badge
- Add button: "Gerar Conta" (creates conta from selected compra)

**CadastrodeCompra.xaml**:
- Show warning if compra is concluída but has no conta_pagar
- Add link: "Criar Conta a Pagar"

### 3. Dashboard Integration

**MainPage / Dashboard** (if exists):
- Add cards: "Contas a Receber Vencidas" and "Contas a Pagar Vencidas"
- Show counts and totals
- Click to navigate to respective screens with filter applied

---

## 📊 Reports (Future Enhancement)

Planned reports (not in initial implementation):

1. **Fluxo de Caixa** (Cash Flow)
   - Income (Contas Receber) vs Expenses (Contas Pagar)
   - By date range
   - Projected vs Actual

2. **Contas Vencidas** (Overdue Accounts)
   - List of overdue receivables/payables
   - Sorted by days overdue
   - PDF export

3. **Histórico de Pagamentos** (Payment History)
   - All payments received/made
   - By date range, customer/supplier

4. **Análise de Inadimplência** (Default Analysis)
   - Customers with most overdue accounts
   - Average days late
   - Total at risk

---

## 🔒 Security & Permissions

**Permission Requirements** (using existing PermissionService):

- **Contas a Receber**:
  - View: `PermissaoContasReceberRead`
  - Create: `PermissaoContasReceberCreate`
  - Edit: `PermissaoContasReceberUpdate`
  - Delete: `PermissaoContasReceberDelete`
  - Receive Payment: `PermissaoContasReceberPagamento`

- **Contas a Pagar**:
  - View: `PermissaoContasPagarRead`
  - Create: `PermissaoContasPagarCreate`
  - Edit: `PermissaoContasPagarUpdate`
  - Delete: `PermissaoContasPagarDelete`
  - Make Payment: `PermissaoContasPagarPagamento`

**Note**: Need to add these permission columns to `usuario` table.

---

## 🧪 Testing Strategy

### Unit Tests (Future)

- Service layer tests for:
  - Status calculation logic
  - Interest/penalty calculation
  - Payment registration
  - Installment generation

### Integration Tests

- End-to-end flows:
  1. Create venda → Generate conta_receber → Generate parcelas → Register payments
  2. Create compra → Generate conta_pagar → Generate parcelas → Register payments
  3. Test overdue status updates (simulate date changes)
  4. Test partial payments
  5. Test cancellations

### Manual Testing Checklist

- [ ] Create conta with 1 installment
- [ ] Create conta with multiple equal installments
- [ ] Create conta with custom installment values
- [ ] Register full payment on parcela
- [ ] Register partial payment on parcela
- [ ] Test overdue status calculation
- [ ] Test interest/penalty calculation
- [ ] Cancel conta (no payments)
- [ ] Cancel conta (with payments) - should fail
- [ ] Filter by status
- [ ] Filter by date range
- [ ] Filter by customer/supplier
- [ ] Dashboard card totals accuracy
- [ ] Navigation from venda/compra screens
- [ ] Prevent duplicate conta for same venda/compra

---

## 📦 Implementation Phases

### Phase 1: Database & Models (1-2 hours)
1. Create SQL schema file
2. Create 4 model classes (ContaReceber, ParcelaReceber, ContaPagar, ParcelaPagar)
3. Create display model classes

### Phase 2: Services Layer (2-3 hours)
1. Implement ContaReceberService
2. Implement ParcelaReceberService
3. Implement ContaPagarService
4. Implement ParcelaPagarService
5. Register services in DI (MauiProgram.cs)

### Phase 3: Contas a Receber UI (3-4 hours)
1. ContasReceberSearch.xaml (list/search screen with dashboard)
2. CadastroContaReceber.xaml (create/edit with installment generator)
3. RegistrarPagamentoParcela.xaml (payment modal)

### Phase 4: Contas a Pagar UI (2-3 hours)
1. ContasPagarSearch.xaml (copy pattern from Receber)
2. CadastroContaPagar.xaml (copy pattern from Receber)
3. RegistrarPagamentoParcelaPagar.xaml (copy pattern from Receber)

### Phase 5: Integration (1-2 hours)
1. Add buttons to VendaSearch/CompraSearch
2. Add links to MaenuPage
3. Add status indicators to existing screens

### Phase 6: Testing & Refinement (1-2 hours)
1. Manual testing all flows
2. Bug fixes
3. UI polish

**Total Estimated Time**: 10-16 hours

---

## 📋 File Structure

```
IntuitERP/
├── models/
│   ├── ContaReceberModel.cs          (NEW)
│   ├── ParcelaReceberModel.cs        (NEW)
│   ├── ContaPagarModel.cs            (NEW)
│   ├── ParcelaPagarModel.cs          (NEW)
│   └── DisplayModels/                (NEW folder)
│       ├── ContaReceberDisplayModel.cs
│       ├── ParcelaReceberDisplayModel.cs
│       ├── ContaPagarDisplayModel.cs
│       └── ParcelaPagarDisplayModel.cs
├── Services/
│   ├── ContaReceberService.cs        (NEW)
│   ├── ParcelaReceberService.cs      (NEW)
│   ├── ContaPagarService.cs          (NEW)
│   └── ParcelaPagarService.cs        (NEW)
├── Viwes/
│   ├── Search/
│   │   ├── ContasReceberSearch.xaml[.cs]  (NEW)
│   │   └── ContasPagarSearch.xaml[.cs]    (NEW)
│   ├── CadastroContaReceber.xaml[.cs]     (NEW)
│   ├── CadastroContaPagar.xaml[.cs]       (NEW)
│   ├── RegistrarPagamentoParcela.xaml[.cs] (NEW)
│   ├── RegistrarPagamentoParcelaPagar.xaml[.cs] (NEW)
│   ├── MaenuPage.xaml[.cs]           (MODIFY - add navigation buttons)
│   ├── Search/VendaSearch.xaml[.cs]  (MODIFY - add "Gerar Conta" button)
│   └── Search/CompraSearch.xaml[.cs] (MODIFY - add "Gerar Conta" button)
└── MauiProgram.cs                    (MODIFY - register services)

Root/
├── CONTAS_DATABASE_SCHEMA.sql        (NEW)
├── CONTAS_MOCK_DATA.sql              (NEW - optional)
└── CONTAS_USAGE_GUIDE.md             (NEW - documentation)
```

**Total New Files**: ~18 files (8 models, 4 services, 6 views)
**Total Modified Files**: ~4 files (MaenuPage, VendaSearch, CompraSearch, MauiProgram)

---

## ⚙️ Configuration via Settings System

Leverage existing SystemSettingsService for:

```csharp
// Add to SETTINGS_DATABASE_SCHEMA.sql
INSERT INTO system_settings VALUES
('contas_juros_mensal_percent', '1.0', 'decimal', 'financial', 'Monthly interest rate for overdue accounts', '1.0'),
('contas_multa_atraso_percent', '2.0', 'decimal', 'financial', 'Late payment penalty percentage', '2.0'),
('contas_carencia_dias', '0', 'int', 'financial', 'Grace period days before interest applies', '0'),
('contas_intervalo_parcelas_dias', '30', 'int', 'financial', 'Default days between installments', '30'),
('contas_alerta_vencimento_dias', '3', 'int', 'financial', 'Days before due date to show alert', '3');
```

---

## 🚨 Known Limitations & Future Enhancements

### Limitations (v1.0):

1. **No payment history tracking** - Only stores last payment per parcela
   - Future: Add `parcela_pagamentos` table for complete history

2. **No recurring accounts** - Each conta is linked to one venda/compra
   - Future: Support independent contas (not linked to transactions)

3. **No batch payment** - Must register payments one parcela at a time
   - Future: Add "Pagar Múltiplas Parcelas" feature

4. **No bank reconciliation** - Manual payment entry only
   - Future: Import bank statements, auto-match

5. **Basic reporting** - No cash flow or analytics
   - Future: Add comprehensive reports module

### Future Enhancements:

1. Email/SMS reminders for upcoming due dates
2. Automatic interest calculation (daily cron job)
3. Payment plans and renegotiation
4. Credit notes and refunds
5. Integration with accounting system
6. Multi-currency support
7. Installment groups (pay multiple contas together)
8. Customer/Supplier account statements
9. Payment gateway integration (boleto, PIX)
10. Mobile app for payment registration

---

## ✅ Definition of Done

Implementation is complete when:

- [ ] All 4 database tables created and tested
- [ ] All 8 model classes implemented with proper validation
- [ ] All 4 service classes implemented with CRUD + business logic
- [ ] All 6 views implemented matching existing UI patterns
- [ ] Navigation integrated in MaenuPage, VendaSearch, CompraSearch
- [ ] Services registered in DI container
- [ ] Status auto-calculation working correctly
- [ ] Installment generator working (equal and custom)
- [ ] Payment registration working (full and partial)
- [ ] Interest/penalty calculation working
- [ ] Dashboard cards showing accurate totals
- [ ] All filters working (status, date range, customer/supplier)
- [ ] Manual testing checklist 100% passed
- [ ] Code committed and pushed to branch
- [ ] Documentation created (CONTAS_USAGE_GUIDE.md)

---

## 🎯 Success Metrics

Post-implementation, the system should enable users to:

1. ✅ Generate receivable/payable accounts from sales/purchases
2. ✅ Split payments into installments (custom or equal)
3. ✅ Track payment status in real-time
4. ✅ Register payments with automatic status updates
5. ✅ Identify overdue accounts at a glance
6. ✅ Calculate interest and penalties automatically
7. ✅ Filter and search accounts efficiently
8. ✅ View financial summary dashboard

---

## 📝 Questions for Approval/Revision

Before proceeding, please review and confirm:

1. **Database Schema**: Are the table structures and relationships correct?
2. **Status Values**: Are the status enums appropriate (Pendente/Parcial/Pago/Vencido/Cancelado)?
3. **Installment Logic**: Is the equal/custom split approach acceptable?
4. **Interest/Penalty**: Should we implement auto-calculation or manual entry only?
5. **Payment Methods**: Should we use the payment_methods from settings system?
6. **Permissions**: Should we add new permission columns to usuario table?
7. **UI Layout**: Does the proposed layout match your vision?
8. **Integration**: Should we add navigation buttons to existing screens?
9. **Reports**: Should we include basic reports in v1.0 or defer?
10. **Scope**: Any features missing or out of scope?

---

**Status**: 🔍 AWAITING YOUR APPROVAL

Please review this plan and provide:
- ✅ Approval to proceed as-is
- 🔧 Requested revisions/changes
- ❌ Concerns or blockers

Once approved, I'll begin Phase 1 implementation immediately.

---

**Last Updated**: 2025-11-13
**Author**: Claude (Anthropic)
**Version**: 1.0 - Initial Plan
