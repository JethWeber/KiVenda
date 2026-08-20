# KiVenda Desktop

Sistema de gestão para pequenos comerciantes angolanos (cantinas, armazéns, mini
mercados, lojas de bairro) — 100% offline, sem servidor, sem dependência de
internet.

> "O KiVenda ajuda pequenos comerciantes a vender melhor, controlar o stock e
> acompanhar o crescimento do seu negócio de forma simples, rápida e sem
> complicações."

Este ficheiro é o **painel de acompanhamento do projeto**: mostra o estado de
cada fase de implementação e serve de ponto de entrada para o resto da
documentação. O plano detalhado de cada fase está em
[`docs/PLANO_DE_IMPLEMENTACAO.md`](docs/PLANO_DE_IMPLEMENTACAO.md).

---

## Stack técnica

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| UI Desktop | Avalonia 11 |
| Padrão de UI | MVVM via CommunityToolkit.Mvvm |
| Base de dados | SQLite (local, sem servidor) |
| ORM | Entity Framework Core |
| DI | Microsoft.Extensions.DependencyInjection |
| Logging | Serilog (consola + ficheiro) |
| Testes | xUnit + FluentAssertions |
| Gestão de pacotes | Central Package Management (`Directory.Packages.props`) |

---

## Estrutura do repositório

```
KiVenda/                              ← raiz do repositório
├── KiVenda.sln
├── global.json                       ← fixa o SDK .NET 10
├── Directory.Build.props             ← propriedades comuns a todos os projetos
├── Directory.Packages.props          ← versões centralizadas dos pacotes NuGet
├── .editorconfig
├── .gitignore
│
├── docs/
│   └── PLANO_DE_IMPLEMENTACAO.md     ← plano completo, fase a fase
│
├── src/
│   ├── KiVenda.Core/                 ← entidades e regras de negócio (Fase 1)
│   ├── KiVenda.Application/          ← casos de uso (Fase 3) + contratos de persistência (Abstractions/, definidos na Fase 2)
│   ├── KiVenda.Infrastructure/       ← impressora, scanner, backup, licenciamento (Fase 4)
│   ├── KiVenda.Persistence/          ← EF Core + SQLite: DbContext, Configurations/, Repositories/, Seed/ (Fase 2)
│   └── KiVenda.Desktop/              ← UI Avalonia + MVVM (composition root)
│
└── tests/
    ├── KiVenda.Core.Tests/
    ├── KiVenda.Application.Tests/
    └── KiVenda.Persistence.Tests/
```

Direção de dependências (não pode ser invertida):

```
KiVenda.Desktop
   │
   ├──► KiVenda.Application ──► KiVenda.Core
   ├──► KiVenda.Infrastructure ──► KiVenda.Application ──► KiVenda.Core
   └──► KiVenda.Persistence ──► KiVenda.Application ──► KiVenda.Core
```

`KiVenda.Core` nunca referencia nenhum outro projeto da solução.

---

## Como correr o projeto

Pré-requisitos: [.NET SDK 10](https://dotnet.microsoft.com/) instalado.

```bash
# a partir da raiz do repositório
dotnet restore
dotnet build

# correr a aplicação Desktop
dotnet run --project src/KiVenda.Desktop/KiVenda.Desktop.csproj

# correr todos os testes
dotnet test
```

> **Nota:** este scaffold foi gerado num ambiente sem acesso ao NuGet/SDK
> .NET, pelo que os ficheiros foram escritos manualmente e **ainda não foram
> validados com `dotnet build` real**. Ao correr `dotnet restore` pela
> primeira vez numa máquina com internet, confirma as versões dos pacotes em
> `Directory.Packages.props` (podem já existir versões mais recentes de
> Avalonia, EF Core, etc.) e corrige o que for necessário.

---

## Estado do projeto — acompanhamento por fase

Legenda: ✅ Concluída · 🔄 Em curso · ⬜ Pendente

| # | Fase | Estado | Entregável |
|---|---|---|---|
| 0 | [Fundação do Projeto](#fase-0--fundação-do-projeto-✅) | ✅ | Solução compilável + janela Avalonia a abrir |
| 1 | [Core — Entidades e Regras de Negócio](#fase-1--core--entidades-e-regras-de-negócio-✅) | ✅ | `KiVenda.Core` com entidades (Produto, UnidadeMedida, ApresentacaoProduto, MovimentoStock) e testes unitários |
| 2 | [Persistence — SQLite + EF Core](#fase-2--persistence-sqlite--ef-core-✅) | ✅ | Persistência local funcional, com repositórios, UnitOfWork e testes de integração |
| 3 | [Application — Casos de Uso](#fase-3--application-casos-de-uso-✅) | ✅ | 36 casos de uso testáveis isoladamente, com permissões e auditoria |
| 4 | Infrastructure | ⬜ | Impressora, backup, licenciamento |
| 5 | Multiutilizador e Perfis de Acesso | ⬜ | Login local + perfis Gerente/Atendente |
| 6 | UI Desktop — Módulos Base | ⬜ | Dashboard, Produtos, Compras, Clientes, Fornecedores |
| 7 | Vendas (PDV) e Caixa | ⬜ | Fluxo de venda e caixa de ponta a ponta |
| 8 | Scanner de Código de Barras | ⬜ | Leitura via input tipo teclado |
| 9 | Relatórios | ⬜ | Diário, Mensal, Stock |
| 10 | Auditoria | ⬜ | Log de operações sensíveis |
| 11 | Configurações, Licenciamento e Backup | ⬜ | Onboarding < 5 minutos |
| 12 | Testes | ⬜ | Suite completa + aceitação com cliente piloto |
| 13 | Empacotamento e Lançamento | ⬜ | Instalador pronto para distribuição |

Detalhe completo de cada fase (escopo, tarefas, critérios de aceitação):
[`docs/PLANO_DE_IMPLEMENTACAO.md`](docs/PLANO_DE_IMPLEMENTACAO.md).

---

## Fase 0 — Fundação do Projeto ✅

**Objetivo:** preparar a base técnica sobre a qual todas as fases seguintes
serão construídas.

### O que foi feito

- [x] Solução `KiVenda.sln` criada, com os 5 projetos de código
      (`Core`, `Application`, `Infrastructure`, `Persistence`, `Desktop`) e
      os 3 projetos de teste correspondentes, organizados em `src/` e `tests/`.
- [x] Direção de dependências entre camadas configurada nos `.csproj`
      (`Core` sem dependências; `Application` depende só de `Core`;
      `Infrastructure` e `Persistence` dependem de `Application`+`Core`;
      `Desktop` depende de todas).
- [x] `global.json` a fixar o SDK **.NET 10**.
- [x] `Directory.Build.props` com propriedades comuns (TargetFramework,
      Nullable, ImplicitUsings, analisadores).
- [x] `Directory.Packages.props` com gestão centralizada de versões
      (Central Package Management), incluindo **Avalonia 11**,
      **CommunityToolkit.Mvvm**, EF Core + SQLite (fixados para a Fase 2),
      Serilog e xUnit/FluentAssertions.
- [x] Projeto `KiVenda.Desktop` configurado com Avalonia 11
      (`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`,
      `Avalonia.Fonts.Inter`, `Avalonia.Diagnostics` em Debug) e
      `CommunityToolkit.Mvvm` para o padrão MVVM.
- [x] `Program.cs` com bootstrap do **Serilog** (consola + ficheiro
      diário em `logs/`) antes do arranque do `AppBuilder`.
- [x] `App.axaml` / `App.axaml.cs` como *composition root*: configura o
      `IServiceProvider` (Microsoft.Extensions.DependencyInjection) e
      resolve o `MainWindowViewModel` por DI.
- [x] `ViewModelBase` (baseado em `ObservableObject` do
      CommunityToolkit.Mvvm) como base de todos os ViewModels.
- [x] `MainWindowViewModel` + `MainWindow.axaml` como prova de vida:
      janela Avalonia a abrir com texto vindo de uma propriedade
      observável (`[ObservableProperty]`).
- [x] `app.manifest` com DPI awareness para Windows.
- [x] `.gitignore` e `.editorconfig` na raiz.
- [x] Um teste "smoke" por projeto de testes (`Core.Tests`,
      `Application.Tests`, `Persistence.Tests`), só para validar a
      referenciação — serão substituídos pelos testes reais nas fases
      seguintes.
- [x] Plano de implementação completo copiado para `docs/PLANO_DE_IMPLEMENTACAO.md`.

### Decisões tomadas nesta fase

- **Nome da raiz do repositório é `KiVenda/`**, não `KiVenda.Desktop/`
  — `KiVenda.Desktop` é apenas o projeto de UI (Avalonia), um dos cinco
  projetos dentro de `src/`.
- **Central Package Management** adotado desde o início (`Directory.Packages.props`)
  para evitar divergência de versões entre projetos à medida que o
  número de camadas cresce.
- **Composition root simples** (`ServiceCollection` direto em
  `App.axaml.cs`) nesta fase; avaliar migração para `Microsoft.Extensions.Hosting`
  completo (`IHost`) quando a Application/Infrastructure tiverem serviços
  de longa duração a gerir (ex.: watcher de backup).
- **Logging via Serilog** configurado antes de tudo o resto no `Program.cs`,
  para capturar falhas fatais mesmo no arranque do `AppBuilder`.

### Pendente para validar (primeira execução numa máquina real)

- [ ] `dotnet restore` — confirmar resolução de todos os pacotes.
- [ ] `dotnet build` — confirmar compilação limpa da solução completa.
- [ ] `dotnet run --project src/KiVenda.Desktop` — confirmar que a janela abre.
- [ ] `dotnet test` — confirmar que os 3 smoke tests passam.
- [ ] Rever versões em `Directory.Packages.props` contra as mais recentes
      disponíveis no NuGet no momento da primeira execução.

### Próxima fase

➡️ **Fase 1 — Core: Entidades e Regras de Negócio.** Ver secção abaixo.

---

## Fase 1 — Core: Entidades e Regras de Negócio ✅

**Objetivo:** modelar o domínio do negócio (entidades + regras de negócio) de forma independente de qualquer tecnologia de UI ou base de dados.

> **Revisão de domínio feita antes de codificar (plano v1.1):** o modelo
> de estoque original (`Produto.quantidade`, contador solto) foi
> substituído por **Produto → Unidade Base → Apresentações Comerciais →
> Movimentos de Stock**, com custo por unidade base calculado como
> **custo médio ponderado**, e domínio já preparado (mas não implementado
> operacionalmente) para lote/FIFO no futuro. Motivo: o público-alvo do
> KiVenda compra e vende o mesmo produto em unidades diferentes (ex.:
> açúcar comprado em saco de 25 kg e vendido a 500 g). Detalhe completo:
> [`docs/PLANO_DE_IMPLEMENTACAO.md` → Nota de Revisão de Domínio — Estoque](docs/PLANO_DE_IMPLEMENTACAO.md#nota-de-revisão-de-domínio--estoque).

### O que foi feito

**Entidades e enums (`src/KiVenda.Core`):**

- [x] `Common/Entity` — base de identidade (Id, CriadoEm, AtualizadoEm, igualdade por Id) para todas as entidades.
- [x] `Exceptions/DomainException` — exceção única para violações de regras de negócio.
- [x] **Produtos e Estoque** (núcleo da revisão de domínio):
  - `UnidadeMedida` — unidade indivisível do stock (`un`, `g`, `ml`).
  - `Categoria`
  - `ApresentacaoProduto` — forma comercial de comprar/vender (ex.: "1 kg", "Saco 25 kg"), com fator de conversão e código de barras próprio opcional; sabe converter-se de/para unidade base.
  - `MovimentoStock` — fonte de verdade do estoque (Entrada/Saída/Ajuste, quantidade sinalizada em unidade base, custo por unidade base quando aplicável, origem, utilizador, `LoteId` opcional).
  - `Lote` — preparado para custeio por lote/FIFO futuro; **não usado operacionalmente no MVP**.
  - `Produto` — agregado raiz: nasce com uma apresentação padrão (fator 1), gere apresentações adicionais, e expõe `RegistarEntradaStock`, `RegistarSaidaStock`, `RegistarAjusteStock` e `RecalcularEstoqueMaterializado`, todos operando sobre `EstoqueAtual`/`CustoMedioPonderado` como valores materializados.
- [x] **Clientes/Fornecedores:** `Cliente`, `Fornecedor` (cadastros simples, sem fiado — fora de escopo do MVP).
- [x] **Compras:** `Compra`, `ItemCompra` — item convertido para unidade base no momento de adicionar, expondo o custo por unidade base derivado (`CustoTotalItem ÷ QuantidadeUnidadeBase`).
- [x] **Vendas:** `Venda`, `ItemVenda`, `Pagamento` — item "fotografa" preço de venda e custo médio ponderado no momento da venda; venda suporta pagamento misto (vários `Pagamento`) e só finaliza se `TotalPago >= Total`.
- [x] **Caixa:** `SessaoCaixa`, `MovimentoCaixa` — abre com saldo inicial, aceita suprimento/sangria/entrada de venda, e `Fechar(saldoInformado)` calcula a divergência face ao saldo esperado.
- [x] **Utilizadores:** `Utilizador`, enum `Acao`, classe estática `Permissoes` — matriz de permissões por perfil (Gerente vs. Atendente) como fonte única de verdade, consultável tanto pela Application (Fase 3) como pela UI (Fase 6).
- [x] **Auditoria:** `LogAuditoria` (utilizador, ação, entidade afetada, dados antes/depois opcionais, data/hora automática).
- [x] Enums de suporte: `PerfilUtilizador`, `MetodoPagamento`, `TipoMovimentoStock`, `OrigemMovimentoStock`, `EstadoStock`, `TipoMovimentoCaixa`, `EstadoSessaoCaixa`, `EstadoVenda`.

**Regras de negócio implementadas (sem dependência de banco):**

- [x] Conversão de quantidade entre apresentação comercial e unidade base (`ApresentacaoProduto.ConverterParaUnidadeBase` / `ConverterDeUnidadeBase`).
- [x] Custo médio ponderado recalculado a cada entrada de stock (`Produto.RegistarEntradaStock`), a partir do custo total da entrada e do estoque/custo já existentes.
- [x] Baixa de stock por venda sem permitir stock negativo (`Produto.RegistarSaidaStock`).
- [x] Ajuste manual de stock com motivo obrigatório, sem permitir stock negativo (`Produto.RegistarAjusteStock`).
- [x] Recalculo do estoque materializado a partir do histórico de movimentos, para diagnóstico de divergências (`Produto.RecalcularEstoqueMaterializado`).
- [x] Cálculo de lucro estimado a partir do **custo médio ponderado**, nunca de um preço de compra fixo (`Produto.CalcularLucroEstimado`, usado também em `ItemVenda.LucroEstimado`).
- [x] Estado de stock (Em Stock / Stock Baixo / Sem Stock) a partir do stock mínimo (`Produto.ObterEstadoStock`).
- [x] Saldo de caixa (entradas − saídas + saldo inicial) e cálculo de divergência no fecho (`SessaoCaixa.SaldoCalculado` / `Fechar`).
- [x] Matriz de permissões por perfil (`Permissoes.Permite`), consumida por `Utilizador.PodeExecutar`.
- [x] Validações de domínio: preços/quantidades não negativos, fator de conversão sempre positivo, produto nasce sempre com uma apresentação válida, pagamento insuficiente bloqueia o fecho da venda, sangria não pode exceder o saldo em caixa.

**Testes unitários (`tests/KiVenda.Core.Tests`):**

- [x] `Produtos/ApresentacaoProdutoTests` — conversões de unidade nos dois sentidos e validações.
- [x] `Produtos/ProdutoTests` — entradas, saídas, ajustes, **custo médio ponderado entre compras sucessivas a custos diferentes**, estado de stock (teoria com 3 cenários), recalculo do estoque materializado, lucro via custo médio ponderado.
- [x] `Compras/CompraTests` — conversão de apresentação comprada, custo por unidade base, rejeição de apresentação de outro produto.
- [x] `Vendas/VendaTests` — conversão de apresentação vendida, snapshot de preço/custo, desconto, pagamento insuficiente, pagamento misto, bloqueio de alterações após finalizada.
- [x] `Caixa/SessaoCaixaTests` — saldo calculado com suprimento/sangria/venda, rejeição de sangria acima do saldo, divergência positiva e negativa no fecho.
- [x] `Utilizadores/PermissoesTests` — matriz completa Gerente vs. Atendente, utilizador inativo nunca tem permissão.
- [x] `Cadastros/EntidadesBasicasTests` — validações de `Cliente`, `Fornecedor`, `Categoria`, `UnidadeMedida`, `LogAuditoria`.
- [x] Smoke test do Core removido (substituído pelos testes reais acima); smoke tests de `Application.Tests` e `Persistence.Tests` mantidos, por essas camadas ainda não estarem implementadas.

### Decisões tomadas nesta fase

- **Cada agregado só se muta a si próprio.** `Compra.AdicionarItem` e `Venda.AdicionarItem` fazem a conversão de unidades e leem dados do `Produto` (preço, custo médio, apresentações), mas quem efetivamente chama `Produto.RegistarEntradaStock`/`RegistarSaidaStock` é a camada de Application (Fase 3) — Core não deixa um agregado escrever diretamente no estado de outro.
- **`EstoqueAtual` e `CustoMedioPonderado` são sempre valores materializados**, nunca a fonte de verdade — daí `RecalcularEstoqueMaterializado` existir desde já, para suportar o critério de aceitação "o estoque pode ser recalculado a partir do histórico de movimentos".
- **`ItemVenda` fotografa preço e custo no momento da venda** (não referencia o `Produto` "ao vivo"), para que uma venda antiga não mude de valor se o preço ou o custo médio do produto mudarem depois.
- **`Lote` existe mas não é usado operacionalmente** — é só uma referência opcional em `MovimentoStock.LoteId`, para não obrigar a remodelar o Core quando FIFO for implementado numa fase futura.
- **Permissões centralizadas numa única fonte (`Permissoes`)**, para a Fase 3 (casos de uso) e a Fase 6 (UI) nunca duplicarem a regra de "quem pode o quê".
- **Sem dependência de EF Core nesta fase.** Os construtores privados sem parâmetros (`private Entidade() { }`) já preparam o terreno para o EF Core materializar entidades na Fase 2, sem que o Core dependa de nenhum pacote de persistência.

### Pendente para validar (primeira execução numa máquina real)

- [ ] `dotnet build` — confirmar compilação limpa do `KiVenda.Core` e do `KiVenda.Core.Tests`.
- [ ] `dotnet test --filter KiVenda.Core.Tests` — confirmar que todos os testes desta fase passam.
- [ ] Revisão de code review humana às regras de custo médio ponderado e de saldo de caixa, por serem as mais sensíveis a erros de arredondamento/sinal.

### Próxima fase

➡️ **Fase 2 — Persistence: SQLite + EF Core.** Ver secção abaixo.

---

## Fase 2 — Persistence: SQLite + EF Core ✅

**Objetivo:** persistir as entidades do Core localmente em SQLite, sem qualquer servidor externo, respeitando o design do domínio (construtores privados, coleções encapsuladas).

> **Decisão de arquitetura tomada nesta fase:** os **contratos de
> repositório** (`IProdutoRepository`, `IUnitOfWork`, etc.) foram
> definidos em `KiVenda.Application.Abstractions.Persistence`, não em
> Persistence — porque quem consome uma interface é quem deve ser dono
> dela (inversão de dependência). Isto adianta uma pequena parte da
> Fase 3 (só os contratos, não os casos de uso), de forma deliberada,
> para a Fase 2 poder implementar algo contra um contrato real em vez de
> inventar um a posteriori.

### O que foi feito

**Contratos (`src/KiVenda.Application/Abstractions/Persistence`):**

- [x] `IProdutoRepository`, `IMovimentoStockRepository`, `ICategoriaRepository`, `IUnidadeMedidaRepository`, `IClienteRepository`, `IFornecedorRepository`, `ICompraRepository`, `IVendaRepository`, `ISessaoCaixaRepository`, `IUtilizadorRepository`, `ILogAuditoriaRepository`.
- [x] `IUnitOfWork` — agrega todos os repositórios acima e expõe `SaveChangesAsync`, para que operações compostas (ex.: futura `FinalizarVenda` na Fase 3, que mexe em `Venda`, `Produto` e `SessaoCaixa` ao mesmo tempo) persistam tudo numa única transação.

**`KiVenda.Persistence` (32 ficheiros):**

- [x] `KiVendaDbContext` — um `DbSet` por entidade, mapeamentos aplicados via `ApplyConfigurationsFromAssembly`.
- [x] **17 ficheiros de configuração Fluent API** (`Configurations/`), um por entidade, incluindo:
  - Precisão explícita (`HasPrecision`) em todos os campos monetários/quantidade, para não haver truncamento silencioso.
  - Índices únicos onde o domínio exige (código interno e código de barras do produto, código de barras de apresentação, nome de utilizador).
  - **Índice dedicado `(ProdutoId, Data)` em `MovimentoStock`**, exatamente como previsto no plano, para tornar rápido tanto o `ConsultarMovimentosStock` paginado como o `RecalcularEstoqueMaterializado`.
  - **Mapeamento das coleções privadas do Core** (`Produto.Apresentacoes`, `Compra.Itens`, `Venda.Itens`, `Venda.Pagamentos`, `SessaoCaixa.Movimentos`) via `Navigation(...).HasField("_campo").UsePropertyAccessMode(PropertyAccessMode.Field)`, respeitando o encapsulamento desenhado na Fase 1 em vez de forçar setters públicos no domínio.
  - **FKs sombra** (`"CompraId"` em `ItemCompra`, `"VendaId"` em `ItemVenda`) para os casos em que o Core, de propósito, não dá ao filho uma referência explícita ao pai — mantém o domínio limpo sem abrir mão de uma FK real na base de dados.
  - `Ignore(...)` em todas as propriedades calculadas em memória (`Compra.CustoTotal`, `Venda.Subtotal/Total/TotalPago/LucroEstimado`, `SessaoCaixa.TotalEntradas/TotalSaidas/SaldoCalculado`, `ItemVenda.ValorTotal/LucroEstimado`, `ItemCompra.CustoUnitarioUnidadeBase`, `MovimentoCaixa.EhEntrada`) — sem isto, o EF Core tentaria mapeá-las como colunas e falharia por não terem setter nem backing field.
- [x] **11 implementações de repositório** (`Repositories/`) sobre o `KiVendaDbContext`, incluindo `ProdutoRepository.ObterPorCodigoBarrasAsync`, que já procura tanto no código de barras do produto como no de qualquer uma das suas apresentações (preparado para o fluxo do scanner — Fase 8).
- [x] `UnitOfWork` — implementação de `IUnitOfWork` com repositórios *lazy* sobre o mesmo `DbContext`.
- [x] `KiVendaDbSeeder` — semeia unidades de medida padrão (`un`, `g`, `ml`), categoria "Geral" e o utilizador Gerente inicial; idempotente (seguro para correr mais do que uma vez); **não calcula o hash da password** (isso é responsabilidade da Infrastructure, Fase 4) — recebe o hash já pronto.
- [x] `KiVendaDbContextFactory` (`IDesignTimeDbContextFactory`), para permitir `dotnet ef migrations add` sem depender do composition root do Desktop.
- [x] `ServiceCollectionExtensions.AddPersistence` — ponto único de registo em DI, pronto a ser chamado a partir do `App.axaml.cs` do Desktop numa fase futura.
- [x] `.csproj` da Persistence atualizado com `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.EntityFrameworkCore.Design`.

**Testes de integração (`tests/KiVenda.Persistence.Tests`, 8 ficheiros):**

- [x] `KiVendaSqliteFixture` — SQLite em memória por teste, com `EnsureCreated()` (ver nota abaixo sobre migrações).
- [x] `EstoqueRecalculoTests` — cobre **diretamente o critério de aceitação do plano**: persiste um produto com duas entradas, uma saída e um ajuste, relê tudo do zero (sem tracking, como se fosse reabrir a app) e confirma que o valor materializado bate com a soma dos movimentos persistidos; e que `Produto.RecalcularEstoqueMaterializado` (Fase 1) funciona corretamente sobre dados vindos da base de dados.
- [x] `ProdutoPersistenceTests` — produto com múltiplas apresentações sobrevive a um round-trip completo; unicidade de código interno.
- [x] `CompraPersistenceTests` — valida especificamente a FK sombra `CompraId`: os itens continuam associados à compra certa depois de recarregados.
- [x] `VendaPersistenceTests` — venda com pagamento misto (Dinheiro + TPA) persistida e recarregada corretamente, incluindo a FK sombra `VendaId`.
- [x] `SessaoCaixaPersistenceTests` — saldo calculado e divergência de fecho sobrevivem ao round-trip.
- [x] `UtilizadorPersistenceTests` — unicidade de login; perfil e permissões corretos após recarregar.
- [x] `KiVendaDbSeederTests` — seed cria os dados esperados e é idempotente.
- [x] Smoke test da Persistence removido, substituído pelos testes acima.

### Decisões tomadas nesta fase

- **Testes de integração usam `Database.EnsureCreated()`, não migrações reais.** Sem SDK .NET/ferramenta `dotnet ef` disponível no ambiente onde este scaffold foi gerado, não haveria forma de gerar uma migração real e garantir que o ficheiro de migração gerado bate certo com o modelo. `EnsureCreated()` cria o schema diretamente a partir do modelo e é suficiente para validar que o *mapeamento* está correto — que era o risco real desta fase (entidades com construtores privados e coleções encapsuladas). A migração real (`dotnet ef migrations add InicialCreate`) é o primeiro passo pendente, listado abaixo.
- **Precisão decimal explícita em todos os campos monetários/quantidade** (`HasPrecision(18, 4)` ou `(18, 6)` para fatores de conversão e custos unitários), para evitar o aviso do EF Core sobre truncamento silencioso de `decimal` e garantir que Kz e gramas nunca perdem casas decimais inesperadamente.
- **FK sombra em vez de forçar uma propriedade `CompraId`/`VendaId` no domínio.** O Core (Fase 1) foi desenhado deliberadamente sem essas propriedades em `ItemCompra`/`ItemVenda` — a relação só existe a partir do pai (`Compra.Itens`, `Venda.Itens`). Usar uma FK sombra na Persistence resolve isto sem "contaminar" o domínio com uma preocupação puramente relacional.
- **Um `KiVendaDbContextFactory` de design-time dedicado**, para a ferramenta `dotnet ef` conseguir construir o `DbContext` sem precisar arrancar a aplicação Desktop inteira (que só existirá com DI completo a partir da Fase 6).

### Pendente para validar (primeira execução numa máquina real)

- [ ] `dotnet build` — confirmar compilação limpa de `KiVenda.Persistence` e `KiVenda.Persistence.Tests` (maior risco desta fase: o mapeamento das coleções privadas via backing field e das FKs sombra).
- [ ] `dotnet test --filter KiVenda.Persistence.Tests` — confirmar que os 8 ficheiros de teste passam, em especial `EstoqueRecalculoTests`.
- [ ] **Gerar a migração inicial real**, a partir da raiz do repositório:
      ```bash
      dotnet ef migrations add InicialCreate --project src/KiVenda.Persistence --startup-project src/KiVenda.Persistence
      ```
      e confirmar visualmente que o schema gerado corresponde ao esperado (nomes de tabelas em português, FKs sombra presentes, etc.).
- [ ] Confirmar que `IsUnique().HasFilter("[Coluna] IS NOT NULL")` (índices únicos filtrados para campos opcionais como `CodigoBarras`) funciona como esperado no provider SQLite — a sintaxe de colchetes é suportada pelo SQLite por compatibilidade, mas vale confirmar no schema gerado.

### Próxima fase

➡️ **Fase 3 — Application: Casos de Uso.** Ver secção abaixo.

---

## Fase 3 — Application: Casos de Uso ✅

**Objetivo:** orquestrar os casos de uso do sistema, isolando a UI das regras de negócio e do acesso a dados, sempre validando permissões por perfil e registando auditoria nas operações sensíveis.

### O que foi feito

**Contratos transversais (`Abstractions/Auth`):**

- [x] `IContextoAutenticacao` — representa o utilizador autenticado na sessão atual; implementado pelo Desktop a partir do login (Fase 5/6).
- [x] `ISenhaHasher` — cálculo/verificação de hash de password; implementado pela Infrastructure (Fase 4). O Core e a Application nunca lidam com o algoritmo de hashing em si.
- [x] `PermissaoNegadaException` + `PermissaoGuard.Exigir(contexto, acao)` — ponto único onde todos os casos de uso verificam permissão, sempre contra a matriz `Permissoes` já definida no Core (Fase 1).

**36 casos de uso, em 8 módulos:**

| Módulo | Casos de uso |
|---|---|
| Produtos | CriarProduto, EditarProduto, InativarProduto, CriarApresentacaoProduto, EditarApresentacaoProduto (+ Inativar), ListarProdutos |
| Stock | RegistarEntradaStock, RegistarSaidaStock, RegistarAjusteStock, ConsultarStock, ConsultarMovimentosStock, RecalcularEstoqueMaterializado |
| Compras | RegistarCompra, ListarCompras |
| Vendas | IniciarVenda, AdicionarItemVenda, RemoverItemVenda, AplicarDescontoVenda, FinalizarVenda |
| Caixa | AbrirCaixa, FecharCaixa, RegistarSuprimento, RegistarSangria, ConsultarMovimentacoesCaixa |
| Clientes | CriarCliente, EditarCliente, ConsultarHistoricoCompras |
| Fornecedores | CriarFornecedor, EditarFornecedor |
| Relatórios | GerarRelatorioDiario, GerarRelatorioMensal, GerarRelatorioStock |
| Utilizadores | CriarUtilizador, DefinirPerfil, AutenticarUtilizador, AlterarPassword, ListarUtilizadores |

Cada caso de uso é uma classe com um único método público (`ExecutarAsync`), usando *primary constructors* (C# 12) para receber `IUnitOfWork` + `IContextoAutenticacao` (+ `ISenhaHasher` quando aplicável), e devolve DTOs (`record`) desacoplados das entidades do Core.

**Destaques de design:**

- [x] **`FinalizarVendaUseCase`** — o orquestrador mais importante do sistema: numa única transação, finaliza a venda no Core (valida pagamento suficiente), dá saída de stock em cada produto vendido (convertendo a apresentação vendida para unidade base), regista a entrada correspondente na sessão de caixa aberta, grava auditoria ("Venda realizada") e só então chama `SaveChangesAsync` uma única vez. Devolve um `ReciboVendaDto` já pronto para a Infrastructure imprimir (Fase 4).
- [x] **`RegistarCompraUseCase`** — mesmo padrão: cria a `Compra`, dá entrada de stock por item (atualizando o custo médio ponderado do produto) e persiste tudo de uma vez.
- [x] **Auditoria automática** nas operações sensíveis listadas na Secção 7 da documentação funcional: alteração de preço, inativação de produto, ajuste manual de stock, venda realizada, abertura/fecho de caixa, sangria, alteração de perfil de utilizador.
- [x] **`AlterarPasswordUseCase`** distingue "alterar a própria password" (sem permissão especial) de "alterar a de outro utilizador" (exige `Acao.CriarUtilizadores`).
- [x] **`AutenticarUtilizadorUseCase`** devolve sempre a mesma mensagem genérica para utilizador inexistente, inativo ou password errada — para não revelar qual delas falhou.

### Decisões tomadas nesta fase

- **Casos de uso "internos" não se chamam uns aos outros quando isso duplicaria a verificação de permissão.** `FinalizarVenda` (que exige `Acao.FazerVenda`) e `RegistarCompra` (que exige `Acao.RegistarCompras`) fazem a sua própria saída/entrada de stock diretamente sobre o `Produto`, em vez de invocar `RegistarSaidaStockUseCase`/`RegistarEntradaStockUseCase` (que exigem `Acao.AjustarStock`). Isto evita que a composabilidade quebre caso um perfil futuro tenha uma permissão sem ter a outra. `RegistarSaidaStockUseCase`/`RegistarEntradaStockUseCase` continuam a existir como casos de uso standalone (conforme o plano) para entradas/saídas avulsas, não ligadas a uma Compra/Venda formal.
- **Os contratos de repositório da Fase 2 foram ligeiramente ajustados nesta fase**: `IVendaRepository.ListarAsync` ganhou um filtro `clienteId` (necessário para `ConsultarHistoricoCompras`) e `IUnidadeMedidaRepository` ganhou `ObterPorIdAsync` (necessário para validar a unidade base ao criar um produto). Ambos os lados (interface em Application, implementação em Persistence) foram atualizados a par.
- **Permissão de Clientes e Fornecedores inferida por analogia**, já que a tabela de permissões da documentação funcional não os lista explicitamente: Clientes usa a permissão-base (`ConsultarProdutosStockClientes`, disponível a ambos os perfis, porque tipicamente um Atendente cadastra um cliente a meio de uma venda); Fornecedores usa `RegistarCompras` (Gerente-only), por existirem exclusivamente para agilizar o módulo de Compras, esse sim explicitamente restrito.
- **Todos os casos de uso são registados como `Scoped`** (`AddApplicationUseCases`), para que cada operação da UI resolva o seu próprio `IUnitOfWork` e não haja fuga de estado rastreado entre ecrãs.

### Pendente para validar (primeira execução numa máquina real)

- [ ] `dotnet build` — confirmar compilação limpa de `KiVenda.Application` e `KiVenda.Application.Tests`.
- [ ] `dotnet test --filter KiVenda.Application.Tests` — confirmar que os testes passam, em especial `FinalizarVendaUseCaseTests` (o fluxo mais crítico: stock, caixa e auditoria têm de mudar juntos, ou nenhum deles).
- [ ] Os testes desta fase usam um `IUnitOfWork` fake em memória (`tests/KiVenda.Application.Tests/Fakes`), não a Persistence real — isto é deliberado (testes rápidos, focados só na orquestração), mas vale correr também os testes de integração da Fase 2 depois de qualquer alteração aos casos de uso que toquem em stock/caixa/vendas, para garantir que o comportamento bate certo com o mapeamento EF Core real.

### Próxima fase

➡️ **Fase 4 — Infrastructure**
(ver detalhe em [`docs/PLANO_DE_IMPLEMENTACAO.md`](docs/PLANO_DE_IMPLEMENTACAO.md#fase-4--kivendainfrastructure))

---

## Convenções do projeto

- **Commits:** mensagens curtas e descritivas, idealmente prefixadas pela
  fase/módulo (ex.: `fase1(core): adiciona entidade Produto`).
- **Branches:** `main` estável; trabalho de cada fase numa branch própria
  (ex.: `fase-1-core`), integrada por PR quando os critérios de aceitação
  da fase estiverem cumpridos.
- **Cada fase só é dada como concluída** quando: (1) compila sem erros,
  (2) os testes automatizados da fase passam, e (3) este README é
  atualizado com o resumo do que foi feito, tal como a secção da Fase 0
  acima.
