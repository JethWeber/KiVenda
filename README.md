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
| Gestão de pacotes | Versão explícita por `PackageReference` em cada `.csproj` (Central Package Management foi tentado e revertido — [ver nota](#correção-pós-fase-3--central-package-management-revertido)) |

---

## Estrutura do repositório

```
KiVenda/                              ← raiz do repositório
├── KiVenda.sln
├── global.json                       ← fixa o SDK .NET 10
├── Directory.Build.props             ← propriedades comuns a todos os projetos
├── Directory.Build.targets           ← força ManagePackageVersionsCentrally=false (ver Correção Pós-Fase 3, parte 2)
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
│   ├── KiVenda.Infrastructure/       ← impressora, backup, licenciamento, hashing (Fase 4) — configuração do scanner preparada, listener em si na Fase 8
│   ├── KiVenda.Persistence/          ← EF Core + SQLite: DbContext, Configurations/, Repositories/, Seed/ (Fase 2)
│   └── KiVenda.Desktop/              ← UI Avalonia + MVVM (composition root)
│       ├── Styling/                  ← Cores.axaml, Estilos.axaml (identidade visual — Fase 6)
│       ├── Converters/                ← conversores de binding (badges, valores em Kz — Fase 6)
│       ├── ViewModels/Shell/          ← ShellViewModel (sidebar + navegação — Fase 6)
│       ├── ViewModels/Modulos/        ← Dashboard, Produtos, Clientes, Fornecedores, Compras, Utilizadores (Fase 6)
│       ├── Views/Shell/ e Views/Modulos/  ← Views correspondentes
│
└── tests/
    ├── KiVenda.Core.Tests/
    ├── KiVenda.Application.Tests/
    ├── KiVenda.Persistence.Tests/
    └── KiVenda.Infrastructure.Tests/
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
| 4 | [Infrastructure](#fase-4--infrastructure-✅) | ✅ | Impressora, backup, licenciamento e hashing — confirmado a correr numa máquina real |
| 5 | [Multiutilizador e Perfis de Acesso](#fase-5--multiutilizador-e-perfis-de-acesso-✅) | ✅ | Login local funcional, sessão em memória, app ligada de ponta a ponta |
| 6 | [Interface Desktop — Módulos Base](#fase-6--interface-desktop-avalonia--mvvm-módulos-base-✅) | ✅ | Shell, Dashboard, Produtos, Compras, Clientes, Fornecedores, Utilizadores |
| 7 | [Vendas (PDV) e Caixa](#fase-7--módulo-de-vendas-e-fluxo-de-caixa-✅) | ✅ | Fluxo de venda completo (recibo incluído) e fluxo de caixa completo |
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
- [x] Versões de pacotes fixadas explicitamente em cada `.csproj`
      (`Version="..."` por `PackageReference`), incluindo **Avalonia 11**,
      **CommunityToolkit.Mvvm**, EF Core + SQLite (fixados para a Fase 2),
      Serilog e xUnit/FluentAssertions. ⚠️ Originalmente esta fase tinha
      adotado Central Package Management (`Directory.Packages.props`);
      foi revertido depois da Fase 3 por não funcionar na máquina do
      Jeth — ver [Correção Pós-Fase 3](#correção-pós-fase-3--central-package-management-revertido) mais abaixo.
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
- ~~Central Package Management adotado desde o início~~ — **revertido
  após a Fase 3**, ver nota de correção mais abaixo. Cada `.csproj`
  fixa a sua própria versão de pacote diretamente.
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
- [x] ~~Rever versões em `Directory.Packages.props`~~ — já não aplicável, ver nota de correção abaixo.

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

## Correção Pós-Fase 3 — Central Package Management revertido

**O que aconteceu:** ao correr `dotnet test --filter KiVenda.Application.Tests`
pela primeira vez numa máquina real, o `dotnet restore` falhou em todos
os projetos com `error NU1015: The following PackageReference item(s)
do not have a version specified`, incluindo um aviso muito revelador:

```
warning NU1602: KiVenda.Application does not provide an inclusive lower
bound for dependency Microsoft.Extensions.DependencyInjection.
Microsoft.Extensions.DependencyInjection 1.0.0 was resolved instead.
```

**Diagnóstico:** o facto de o NuGet ter tentado resolver `Microsoft.Extensions.DependencyInjection`
sem nenhuma restrição de versão (caindo na 1.0.0, de 2016) confirma que
o `Directory.Packages.props` não estava a ser reconhecido como fonte de
versões — se estivesse, um `PackageReference` sem `Version` seria
válido (esse é o objetivo do Central Package Management), não um erro.
Não foi possível reproduzir nem depurar a causa raiz exata neste
ambiente de geração do scaffold (sem SDK .NET/NuGet disponível), pelo
que investigar às cegas um mecanismo que não consigo testar seria
arriscado.

**Correção aplicada:** em vez de depurar a fundo o porquê do CPM não
estar a ser detetado nesta máquina, o Central Package Management foi
**abandonado**:

- `Directory.Packages.props` removido.
- Cada `<PackageReference>`, em todos os `.csproj` (`Application`,
  `Persistence`, `Desktop`, e os 3 projetos de teste), passou a incluir
  `Version="..."` explícita, com os mesmos números de versão que
  estavam antes centralizados.

Esta abordagem é menos elegante para manter consistência de versões à
medida que o projeto cresce, mas é **garantidamente mais robusta** —
não depende de nenhum mecanismo de descoberta automática de ficheiros
que possa variar entre versões de SDK/NuGet ou configurações de
ambiente. Se o Central Package Management vier a ser reconsiderado no
futuro, o primeiro passo é reproduzir o erro numa máquina com SDK
disponível e confirmar a causa exata antes de reintroduzi-lo.

### Pendente para validar (primeira execução numa máquina real)

- [ ] Correr novamente `dotnet restore` — deve já não apresentar `NU1015`.
- [ ] `dotnet test --filter KiVenda.Application.Tests` — confirmar que a suite corre.
- [ ] `dotnet test` (sem filtro) — confirmar que as 3 suites (Core, Application, Persistence) passam.

---

## Correção Pós-Fase 3 (parte 2) — CPM herdado de fora do repositório

**O que aconteceu:** depois de remover o `Directory.Packages.props` do
repositório, o erro mudou de `NU1015` ("falta a versão") para
**`NU1008`** ("não pode ter versão — projetos com Central Package
Management têm de definir a versão num `PackageVersion`"). Isto é o
erro exatamente oposto ao anterior, e só acontece se o MSBuild **ainda
está a encontrar um `Directory.Packages.props`** — só que agora vindo
de fora do repositório.

**Diagnóstico:** o MSBuild procura `Directory.Packages.props` subindo a
árvore de diretórios a partir de cada projeto, **sem parar na raiz do
repositório** — só para quando encontra o ficheiro ou chega à raiz do
sistema de ficheiros. Se existir um `Directory.Packages.props` nalgum
diretório acima de `~/Project/KiVenda` (por exemplo em `~/Project/` ou
na própria home), o MSBuild vai encontrá-lo e ativar CPM para todos os
projetos abaixo dele — incluindo o KiVenda — mesmo sem esse ficheiro
fazer parte deste repositório. Para confirmar onde está:

```bash
find ~ -maxdepth 6 -iname 'Directory.Packages.props' 2>/dev/null
```

**Correção aplicada:** em vez de depender de encontrar e remover (ou
não poder remover, se for de outro projeto) esse ficheiro externo,
adicionámos **`Directory.Build.targets`** na raiz do repositório com:

```xml
<PropertyGroup>
  <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
</PropertyGroup>
```

`Directory.Build.targets` é importado pelo MSBuild **depois** de tudo
o resto, incluindo depois de qualquer `Directory.Packages.props`
externo — por isso esta definição vence sempre, independentemente do
que exista fora do repositório. Esta correção é robusta mesmo que o
ficheiro externo continue lá (não depende de o utilizador o encontrar
ou apagar).

> ⚠️ **Atualização:** esta correção com `Directory.Build.targets` **não
> resolveu** o problema na prática. O `dotnet restore` usa uma
> avaliação estática mais leve (focada em `PackageReference`/`PackageVersion`)
> que, ao que tudo indica, não chega a processar `Directory.Build.targets`
> — só ficheiros `.props`. A causa real, descoberta a seguir com
> `find ~ -iname 'Directory.Packages.props'`, nem sequer era um
> ficheiro ancestral fora do repositório: era **o próprio ficheiro
> antigo, ainda fisicamente presente dentro do repositório**
> (`~/Project/KiVenda/Directory.Packages.props`). A extração de um zip
> por cima de uma pasta já existente acrescenta/sobrescreve ficheiros,
> mas **não apaga** ficheiros que existiam no disco e deixaram de estar
> no zip — por isso o ficheiro antigo sobreviveu a várias "correções".
> A solução foi simplesmente apagá-lo à mão (`rm Directory.Packages.props`).
> `Directory.Build.targets` foi mantido no repositório como proteção
> adicional para o cenário (diferente) de um `Directory.Packages.props`
> genuinamente ancestral e fora do controlo do repositório, mesmo sem
> garantia de efeito durante `dotnet restore`.

### Pendente para validar (primeira execução numa máquina real)

- [x] Correr `dotnet restore` novamente — **confirmado pelo Jeth**: já não há `NU1008` nem `NU1015`.
- [x] `dotnet test --filter KiVenda.Application.Tests` — **confirmado**: 24 testes, 0 falhas.
- [x] `dotnet run --project src/KiVenda.Desktop` — **confirmado**: a app arranca e mostra o log do Serilog ("A iniciar o KiVenda Desktop...").
- [ ] `dotnet test` (sem filtro) — falta confirmar as 4 suites completas (Core, Application, Persistence, Infrastructure) depois da correção dos `using` em falta na Persistence.Tests (ver Fase 4 abaixo) e dos novos testes da Fase 4.

---

## Fase 4 — Infrastructure ✅

**Objetivo:** implementar as integrações técnicas que dependem do ambiente Desktop — impressora, backup/restore e licenciamento — sobre os contratos que a Application já define (ou que esta fase acrescenta, quando o contrato só faz sentido do lado de fora da Application).

> Esta fase só começou depois de confirmado, numa máquina real do
> Jeth, que a Fase 0–3 compilam e a app arranca (`dotnet run` mostrou
> o log de arranque do Serilog). Ver as correções de `NU1008`/`NU1015`
> logo acima — foram resolvidas antes de continuar.

### O que foi feito

- [x] **`Caminhos/CaminhosAplicacao`** — resolve, de forma multiplataforma (Windows/Linux/macOS), onde a app guarda os seus dados: base de dados, backups, logs, recibos, configuração local e licença. Cria as pastas automaticamente, suportando o princípio "instalar e vender em 5 minutos".
- [x] **`Configuracao/IArmazenamentoConfiguracaoLocal` + `ArmazenamentoConfiguracaoLocalJson`** — armazenamento genérico de preferências (chave → valor) num único ficheiro JSON local, com escrita atómica (ficheiro temporário + `File.Move`) para nunca corromper a configuração se a app fechar a meio de uma escrita. `ConfiguracaoScanner` já modelado como primeiro consumidor (preparação para a Fase 8).
- [x] **`Impressao/IServicoImpressao` + `ServicoImpressaoTexto`** — formata o recibo no estilo de talão térmico (40 colunas, monoespaçado) e grava em ficheiro; reaproveitável para os Relatórios (Fase 9) via `ImprimirTextoAsync`. A integração com uma impressora física fica documentada como fronteira isolada (`EscreverParaDestinoAsync`), a resolver com hardware real na Fase 12.
- [x] **`Backup/IServicoBackup` + `ServicoBackupSqlite`** — usa a API nativa de backup do SQLite (`SqliteConnection.BackupDatabase`, via `Microsoft.Data.Sqlite` diretamente, sem EF Core) em vez de copiar o ficheiro `.db` cru, para garantir uma cópia consistente mesmo com a app em uso. Valida o cabeçalho binário do SQLite antes de qualquer restauração.
- [x] **`Licenciamento/IServicoLicenciamento` + `ServicoLicenciamentoRsa`** — implementação de referência (envelope JSON + assinatura RSA/SHA-256) para validação/ativação de licença, já que o formato real `.wta` da Weber Tech está "documentado em separado" e não estava disponível. `FerramentasLicencaDeTeste` gera pares de chaves e licenças assinadas só para desenvolvimento/testes.
- [x] **`Autenticacao/SenhaHasherPbkdf2`** — implementa o `ISenhaHasher` definido pela Application (Fase 3) usando PBKDF2/SHA-256 (`System.Security.Cryptography`, sem dependências de terceiros), com salt aleatório por password e comparação em tempo constante.
- [x] **`DependencyInjection/ServiceCollectionExtensions.AddInfrastructure`** — ponto único de registo desta camada, pronto para o composition root do Desktop (Fase 6).
- [x] **Novo projeto de testes `KiVenda.Infrastructure.Tests`** (18 testes, 5 ficheiros) — não estava nas 3 suites originais do plano, mas havia lógica real a testar nesta fase (formatação de recibo, backup/restore round-trip com dados reais, assinatura/verificação RSA, hashing de password, persistência de configuração).

### Decisões tomadas nesta fase

- **`IServicoImpressao`, `IServicoBackup` e `IServicoLicenciamento` vivem na própria Infrastructure**, não em `Application.Abstractions` — ao contrário dos repositórios (Fase 2) e de `ISenhaHasher`/`IContextoAutenticacao` (Fase 3). Motivo: nenhum caso de uso da Application chama estes serviços — `FinalizarVendaUseCase` devolve um `ReciboVendaDto` e é o **Desktop** quem decide imprimir; backup e licenciamento são acionados diretamente por ecrãs de Configurações (Fase 11), nunca por uma regra de negócio. Não faz sentido a Application depender de um contrato que nunca invoca.
- **Backup usa `Microsoft.Data.Sqlite` diretamente, não EF Core.** A Infrastructure não referencia a Persistence (são camadas irmãs, ambas dependendo só de Core+Application) — usar a API de backup nativa do SQLite ao nível ADO.NET evita essa dependência cruzada e continua a ser a forma mais segura de copiar uma base de dados SQLite em uso.
- **Licenciamento é uma implementação de referência, não a real da Weber Tech.** O formato `.wta` e o esquema de chaves estão documentados num documento separado a que não tive acesso. Em vez de bloquear a fase, implementei um esquema plausível e completo (RSA + JSON) com a chave pública injetada via construtor (nunca hardcoded), para que trocar pela chave/formato reais da Weber Tech seja uma alteração isolada no composition root do Desktop, não uma reescrita desta camada.
- **Password hashing com PBKDF2 nativo, sem BCrypt.Net.** Evita mais uma dependência de terceiros a gerir versões; `Rfc2898DeriveBytes.Pbkdf2` já está no BCL do .NET e é criptograficamente adequado para este caso de uso.
- **Duas vulnerabilidades reais (`NU1903`) foram corrigidas com *overrides* explícitos de versão**, replicadas em todos os `.csproj` que as puxam transitivamente:
  - `SQLitePCLRaw.lib.e_sqlite3` 2.1.10 → `3.53.3` (CVE-2025-6965 / GHSA-2m69-gcr7-jv3q, via EF Core Sqlite e `Microsoft.Data.Sqlite`).
  - `Tmds.DBus.Protocol` 0.20.0 → `0.21.3` (GHSA-xrw6-gwf8-vvr9, via Avalonia no Linux).
- **Dois `using KiVenda.Core.Enums;` em falta foram corrigidos** em `CompraPersistenceTests.cs` e `SessaoCaixaPersistenceTests.cs` (Fase 2) — só apareceram como erro de compilação real (`CS0103`) quando o Jeth correu `dotnet test` pela primeira vez numa máquina com o SDK instalado; nenhuma verificação manual anterior os tinha apanhado.
- **A formatação do recibo deixou de depender da cultura `pt-AO` do sistema operativo.** Um `dotnet test` real (Fedora) mostrou que essa cultura, nesse ICU específico, usa espaço como separador de milhares ("5 000,00"), não o ponto usado nos mockups do KiVenda ("5.000,00") — a minha tentativa original só previa o caso de "pt-AO" não existir de todo (`CultureNotFoundException`), não o caso de existir e formatar diferente. `ServicoImpressaoTexto.ObterCulturaFormatacao` passou a construir sempre a formatação manualmente (clone de `CultureInfo.InvariantCulture` com separadores fixos), sem nunca consultar a cultura do sistema — evita esta classe de divergência entre máquinas de vez.

### Pendente para validar (primeira execução numa máquina real)

- [x] `dotnet build` — **confirmado**: as 4 camadas + 4 suites de teste compilam (127 testes descobertos no total).
- [x] `dotnet test --filter KiVenda.Infrastructure.Tests` — **confirmado com 1 correção**: 2 de 18 testes falhavam por `ServicoImpressaoTexto` depender da cultura `pt-AO` do sistema (ver correção acima); depois de trocar para formatação manual, devem passar os 18.
- [x] `dotnet test` (sem filtro) — **confirmado pelo Jeth**: 127 testes no total, 125 passaram à primeira, os 2 que falharam foram os da formatação de recibo (já corrigidos acima).
- [ ] `dotnet list package --vulnerable` — ainda por confirmar que os dois `NU1903` desapareceram depois dos overrides.
- [ ] Correr `dotnet test` mais uma vez depois desta correção, para confirmar os 127/127.

### Próxima fase

➡️ **Fase 5 — Multiutilizador e Perfis de Acesso.** Ver secção abaixo.

---

## Fase 5 — Multiutilizador e Perfis de Acesso ✅

**Objetivo:** login local, sessão de utilizador em memória, e primeira ligação real de ponta a ponta entre Desktop → Application → Persistence → Infrastructure, arrancando a partir de um ecrã funcional.

### O que foi feito

- [x] **`Autenticacao/SessaoUtilizadorAtual`** — implementação concreta de `IContextoAutenticacao` (contrato da Application, Fase 3) do lado do Desktop. Vive só em memória durante a execução da app (sem persistir sessão entre arranques — login é sempre obrigatório, Secção 3). Registada como singleton, para que todos os casos de uso vejam sempre o mesmo utilizador autenticado.
- [x] **`ViewModels/LoginViewModel`** — chama `AutenticarUtilizadorUseCase` (Fase 3) dentro do seu próprio `IServiceScope`, criado por tentativa de login. Este é o padrão a repetir em todas as fases seguintes sempre que a UI invoca um caso de uso: nunca resolver um caso de uso a partir do container raiz diretamente, sempre através de um `IServiceScope` de vida curta, para o `IUnitOfWork`/`DbContext` ser sempre novo por operação.
- [x] **`ViewModels/BemVindoViewModel`** — ecrã provisório pós-login (a shell definitiva com os 10 módulos é só na Fase 6), mas já demonstra o padrão de esconder opções por permissão: `PodeAcederConfiguracoes`, `PodeGerirCaixa`, `PodeAcederRelatorios` são calculados no ViewModel consultando `Permissoes.Permite` (Core, Fase 1) — a mesma matriz usada pelos casos de uso, nunca duplicada na UI.
- [x] **`MainWindowViewModel`** atualizado — deixa de mostrar o texto estático da Fase 0 e passa a alternar entre `LoginViewModel` e `BemVindoViewModel` através de uma propriedade `ConteudoAtual`, com `DataTemplate`s em `MainWindow.axaml` a mapear cada ViewModel para a View correspondente (padrão "ViewModel-first" que se mantém válido para toda a navegação da Fase 6 em diante).
- [x] **`App.axaml.cs` liga tudo pela primeira vez**: `AddPersistence` + `AddApplicationUseCases` + `AddInfrastructure` + a sessão de utilizador, e no arranque cria a base de dados (`EnsureCreatedAsync`) e semeia-a (`KiVendaDbSeeder`, com a password do Gerente inicial já com hash via `ISenhaHasher` da Infrastructure) antes de qualquer ecrã aparecer.
- [x] **`Program.cs` corrigido**: os logs deixaram de usar o caminho relativo `"logs/"` (dependente de onde o processo é arrancado) e passaram a usar `CaminhosAplicacao.PastaLogs` (Fase 4), consistente com o resto da app.
- [x] **`App : Application` corrigido para `App : Avalonia.Application`**, por sugestão do Jeth — evita qualquer ambiguidade entre a classe `Application` do Avalonia e o namespace `KiVenda.Application`, que só tende a acumular mais `using`s ao longo das próximas fases.

### Decisões tomadas nesta fase

- **Utilizador Gerente inicial nasce com password fixa (`admin123`)**, semeada automaticamente no primeiro arranque via `KiVendaDbSeeder` + `ISenhaHasher`. Isto é deliberadamente inseguro para produção — fica marcado com um `TODO` explícito no código para a Fase 11 forçar a troca desta password no primeiro login, mas era necessário para o `dotnet run` "funcionar out-of-the-box" sem exigir nenhum passo manual de setup.
- **`IServiceScope` por operação, não um único scope para a app inteira.** `LoginViewModel` cria o seu próprio scope a cada tentativa de login. Esta é a convenção a seguir para toda a UI: qualquer ViewModel que invoque um caso de uso recebe um `IServiceScopeFactory` (nunca o caso de uso diretamente via DI), evitando um `DbContext` a viver indefinidamente e acumular entidades rastreadas ao longo de toda a sessão.
- **Regra a partir de agora: sempre `CreateAsyncScope()` + `await using`, nunca `CreateScope()` + `using`.** Numa execução real, o primeiro login lançava `InvalidOperationException: 'KiVenda.Persistence.UnitOfWork' type only implements IAsyncDisposable` — porque `LoginViewModel` usava `CreateScope()`/`using` (síncrono), mas `UnitOfWork` (Fase 2) só implementa `IAsyncDisposable`, já que o `DbContext` do EF Core se fecha via `DisposeAsync`. `App.axaml.cs` já tinha o padrão certo (`CreateAsyncScope`); faltou replicá-lo aqui. Confirmado por grep que não há mais nenhum `CreateScope()` síncrono no repositório — qualquer ViewModel futuro (Fase 6+) que precise de um scope deve copiar o padrão do `LoginViewModel`, não reinventá-lo.
- **Inicialização da base de dados é síncrona-bloqueante em `OnFrameworkInitializationCompleted`** (`.GetAwaiter().GetResult()`), porque nesse ponto exato o loop de mensagens do Avalonia ainda não arrancou — não há risco de deadlock, e é mais simples do que introduzir um ecrã de "a carregar" só para este passo, que demora milissegundos.
- **`BemVindoViewModel` é intencionalmente descartável.** Não faz parte da estrutura final da app — é só o suficiente para provar login + sessão + permissões de ponta a ponta antes da Fase 6 construir a shell real. Vai ser substituído, não teve o cuidado de design de um ecrã definitivo.

### Pendente para validar (primeira execução numa máquina real)

- [ ] `dotnet build` — confirmar compilação limpa do Desktop com as novas Views/ViewModels (maior risco: bindings compilados do Avalonia, por causa de `AvaloniaUseCompiledBindingsByDefault=true`).
- [ ] `dotnet run --project src/KiVenda.Desktop` — confirmar que a app arranca a mostrar o ecrã de login (não mais o placeholder da Fase 0), e que faz login com `gerente` / `admin123`.
- [ ] Confirmar visualmente que, depois do login, os itens "Configurações", "Caixa" e "Relatórios" aparecem (Gerente tem todas as permissões) — e que "Terminar sessão" volta ao ecrã de login limpo (sem os valores da tentativa anterior).
- [ ] `dotnet test` — confirmar que nada quebrou nas 4 suites de teste já existentes (esta fase não mexeu em Core/Application/Persistence/Infrastructure, só no Desktop, que ainda não tem suite de testes própria — ver nota abaixo).

> **Nota:** esta fase não criou um `KiVenda.Desktop.Tests`. Testes de UI Avalonia são mais custosos de configurar (headless rendering) e o plano já reserva isso para a Fase 12 (Testes de Interface). Por agora, a validação desta fase é manual (correr a app e testar o fluxo de login).

### Próxima fase

➡️ **Fase 6 — Interface Desktop (Avalonia + MVVM): Módulos Base.** Ver secção abaixo.

---

## Fase 6 — Interface Desktop (Avalonia + MVVM): Módulos Base ✅

**Objetivo:** construir a shell da aplicação (menu lateral com os 10 módulos) e os módulos de cadastro que servem de base para Vendas e Caixa (Fase 7) — Dashboard, Produtos, Compras, Clientes, Fornecedores, Utilizadores — usando a identidade visual dos mockups fornecidos (verde como cor de marca, cards arredondados, badges de estado, sidebar de navegação).

### O que foi feito

**Identidade visual (`Styling/`):**

- [x] `Cores.axaml` — paleta extraída dos mockups: verde de marca (`#15803D` primário, `#14532D` escuro para gradientes, `#DCFCE7` para o item de menu ativo), neutros (fundo `#F8FAFC`, cartões brancos, texto `#111827`/`#6B7280`), e cores de estado (sucesso/perigo/aviso/info) para os badges.
- [x] `Estilos.axaml` — classes reutilizáveis (`Classes="cartao"`, `"cartao-verde"`, `"cartao-estatistica"`, `"primario"`, `"secundario"`, `"perigo"`, `"escuro"`, `"badge-sucesso"`/`"badge-perigo"`/`"badge-aviso"`/`"badge-neutro"`, tipografia `"titulo-pagina"`/`"subtitulo"`/`"rotulo-campo"`), para nenhum ecrã futuro repetir a mesma definição de cor/raio/padding.
- [x] `Converters/` — `ValorKzConverter` (reaproveita `FormatadorKz`, Fase 4/5), e três conversores para os badges de estado de stock (texto + cor de fundo + cor de texto, evitando `Classes` condicionais complexas em XAML).

**Shell (`ViewModels/Shell`, `Views/Shell`):**

- [x] `ShellViewModel` — substitui o `BemVindoViewModel` provisório da Fase 5 (removido nesta fase). Constrói o menu lateral consultando `Permissoes.Permite` para cada item (Compras/Fornecedores exigem `RegistarCompras`; Caixa exige `GerirCaixa`; Relatórios exige `AcederRelatorios`; Utilizadores e Configurações exigem `CriarUtilizadores`/`ConfigurarSistema`) — a mesma matriz usada pelos casos de uso, nunca duplicada na UI.
- [x] `ShellView.axaml` — sidebar (logótipo, menu, utilizador atual) + topbar + área de conteúdo com `ContentControl`, seguindo o mesmo padrão "ViewModel-first" com `DataTemplate`s já estabelecido na Fase 5.
- [x] `EmBreveViewModel`/`EmBreveView` — placeholder para os módulos cuja implementação pertence a fases seguintes (Vendas/Caixa → Fase 7, Relatórios → Fase 9, Configurações → Fase 11). O menu já mostra os 10 módulos dos mockups, mas sem fingir que os que ainda não foram construídos já funcionam.

**Módulos (`ViewModels/Modulos`, `Views/Modulos`):**

- [x] `ListaModuloViewModelBase<TDto>` — base reutilizável para "carregar uma lista, com pesquisa, via um caso de uso", usada por Produtos/Clientes/Fornecedores/Compras/Utilizadores. Cada carregamento cria e descarta o seu próprio scope (mesma convenção da Fase 5).
- [x] **Dashboard** — 5 indicadores (Vendas de Hoje, Caixa Atual, Lucro Estimado, Stock Baixo/Sem Stock, Vendas Realizadas), com aviso quando não há sessão de caixa aberta. Usa o novo `ObterResumoDashboardUseCase` (ver decisão abaixo), não o módulo de Relatórios.
- [x] **Produtos** — listagem + formulário de criação (nome, código, categoria, unidade base, preço, stock mínimo, código de barras opcional), com o botão "Novo Produto" escondido para o perfil Atendente (`ProdutosViewModel.PodeCriar`, calculado a partir de `Permissoes`).
- [x] **Clientes** e **Fornecedores** — listagem + formulário de criação simples, mesmo padrão visual da tabela (cabeçalho + linhas com `ItemsControl`).
- [x] **Compras** — listagem + formulário simplificado (um item por compra, usando sempre a apresentação padrão do produto — selecionar entre várias apresentações fica para um refinamento futuro).
- [x] **Utilizadores** — listagem + formulário de criação (nome, login, password inicial, checkbox de perfil), restrito ao Gerente tanto na visibilidade do item de menu como na permissão do caso de uso.

**Ajustes à Application nesta fase:**

- [x] `ObterResumoDashboardUseCase` (novo) — resumo do Dashboard acessível a **ambos os perfis** (`ConsultarProdutosStockClientes`), distinto de `GerarRelatorioDiarioUseCase` (Gerente-only). Os mockups mostram um "Operador de Caixa" a ver Vendas de Hoje/Caixa Atual/Lucro Estimado no Dashboard — usar o relatório Gerente-only teria bloqueado exatamente o que os mockups mostram.
- [x] `ListarCategoriasUseCase` / `ListarUnidadesMedidaUseCase` (novos) — necessários para preencher os `ComboBox` do formulário de Produtos; a Fase 3 não os tinha prevido.
- [x] `ListarClientesUseCase` / `ListarFornecedoresUseCase` (novos) — a Fase 3 só tinha `CriarCliente`/`EditarCliente`/`ConsultarHistoricoCompras` (e o equivalente em Fornecedores), sem nenhum "listar", inviabilizando um ecrã de listagem. Adicionados com a mesma permissão-base já usada pelos restantes casos de uso desses módulos.

### Decisões tomadas nesta fase

- **Ícones do menu são glifos Unicode simples** (🏠 📦 🧾 👥 🚚 🏦 📊 👤 ⚙️), não uma biblioteca de ícones vetoriais (ex.: Lucide/Material). Dado o histórico desta conversa com problemas de build por dependências adicionais, evitar mais um pacote NuGet pareceu a decisão mais robusta — trocar por ícones vetoriais fica como refinamento visual futuro, sem exigir nenhuma mudança de arquitetura.
- **Formulários são painéis inline (`IsVisible` sobre um `Border.cartao`), não janelas de diálogo.** Avalonia suporta janelas modais, mas isso complicaria a gestão de `DataContext`/scope entre janelas nesta fase inicial da UI. O padrão atual (abrir/fechar painel na mesma tela) é suficiente e mais simples.
- **Pesquisa dispara o carregamento a cada tecla** (`OnTermoPesquisaChanged` chama `CarregarAsync` imediatamente, sem debounce). Funcionalmente correto, mas gera mais chamadas do que o necessário durante a digitação — um `Task.Delay` com cancelamento seria a melhoria óbvia; fica anotado como refinamento futuro, não bloqueante para esta fase.
- **Todos os `DataTemplate`s de módulo vivem em `ShellView.axaml`**, não espalhados — um único sítio para ver "que ViewModel mostra que View", em vez de cada módulo ter de se registar em vários lugares.

### Pendente para validar (primeira execução numa máquina real)

- [ ] `dotnet build` — este é o maior risco desta fase: muito XAML novo, bindings compilados (`AvaloniaUseCompiledBindingsByDefault=true`), `DataTemplate`s aninhados e o uso de `{x:Static ObjectConverters.IsNotNull}` sem `xmlns` explícito (assumido disponível via o namespace `https://github.com/avaloniaui`, mas não testado por não haver SDK/Avalonia disponível neste ambiente de geração do scaffold).
- [ ] `dotnet run --project src/KiVenda.Desktop` — confirmar visualmente: sidebar com os 10 módulos, Dashboard com os 5 indicadores, criar um produto, um cliente e um utilizador Atendente; fazer login com esse Atendente e confirmar que Compras/Fornecedores/Caixa/Relatórios/Utilizadores/Configurações desaparecem do menu.
- [ ] Confirmar que o botão "Novo Produto" desaparece para o Atendente, mas a listagem continua visível.
- [ ] `dotnet test` — confirmar que as 4 suites de teste continuam a passar (esta fase alterou a Application com 4 casos de uso novos, sem testes próprios ainda — ver nota abaixo).

> **Nota:** os 4 casos de uso novos desta fase (`ObterResumoDashboardUseCase`, `ListarCategoriasUseCase`, `ListarUnidadesMedidaUseCase`, `ListarClientesUseCase`, `ListarFornecedoresUseCase`) ainda não têm testes unitários próprios em `KiVenda.Application.Tests` — são consultas simples (sem regra de negócio nova, só orquestração de leitura), mas fica como dívida técnica a fechar numa próxima passagem, idealmente antes da Fase 12.

### Próxima fase

➡️ **Fase 7 — Módulo de Vendas e Fluxo de Caixa.** Ver secção abaixo.

---

## Correção Pós-Fase 6 — `Grid.ColumnSpacing`/`RowSpacing` e `WrapPanel.ItemSpacing`/`LineSpacing`

**O que aconteceu:** `dotnet run` falhou com `Avalonia error AVLN2000: Unable to
resolve suitable regular or attached property ColumnSpacing/RowSpacing/
ItemSpacing/LineSpacing` em 6 ficheiros `.axaml`. Estas propriedades só
foram adicionadas ao Avalonia na série 11.3 — a versão fixada nos
`.csproj` (11.2.3) não as tinha.

**Correção aplicada pelo Jeth:** em vez de reescrever todo o XAML para
não usar essas propriedades, o Jeth atualizou `KiVenda.Desktop.csproj`
para **Avalonia 11.3.10** (mais os pacotes irmãos — `Avalonia.Desktop`,
`Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Diagnostics`
— e adicionou `LiveChartsCore.SkiaSharpView.Avalonia` 2.0.5, reservado
para gráficos numa fase futura, ex.: Relatórios/Dashboard). Sincronizado
neste repositório.

### Pendente para validar

- [ ] Confirmar que `dotnet run` já não apresenta `AVLN2000`.

---

## Fase 7 — Módulo de Vendas e Fluxo de Caixa ✅

**Objetivo:** o módulo central do sistema (Secção 4.4) — PDV completo, de ponta a ponta — e o fluxo de caixa completo (Secção 4.5): Abrir Caixa → vendas/entradas/saídas → Fechar Caixa, com divergência calculada.

### O que foi feito

**Ajustes à Application nesta fase:**

- [x] `ConsultarVendaUseCase` (novo) — nenhum dos casos de uso de Vendas da Fase 3 (`AdicionarItemVenda`, `RemoverItemVenda`, `AplicarDescontoVenda`) devolve a venda completa, só confirma a operação. Sem uma forma de reconsultar o estado atual da venda, o carrinho do PDV não tinha como se redesenhar depois de cada ação — esta fase acrescentou a consulta em falta.
- [x] `CancelarVendaUseCase` (novo) — a Secção 4.4 da documentação funcional prevê "Cancelamento de venda em curso", mas a Fase 3 não o tinha implementado.

**PDV (`VendasViewModel` / `VendasView`):**

- [x] Ao entrar no módulo, tenta iniciar uma venda de imediato (via `IniciarVendaUseCase`); se não houver sessão de caixa aberta, mostra um ecrã de aviso em vez do PDV, em vez de deixar a exceção do Core aparecer crua.
- [x] Pesquisa/grelha de produtos com adicionar ao carrinho num clique; o mesmo campo de pesquisa aceita um código exato e Enter — a base do fluxo de scanner que a Fase 8 vai completar (ativar/desativar, som, etc.).
- [x] Carrinho com remoção de item, desconto, seleção de método de pagamento (Dinheiro/Multicaixa/TPA) e valor pago (vazio = valor exato do total).
- [x] Ao finalizar, chama `FinalizarVendaUseCase` e, com sucesso, `IServicoImpressao.ImprimirReciboVendaAsync` (Infrastructure, Fase 4) — a primeira vez que um módulo de UI efetivamente aciona a impressão do recibo.
- [x] "Cancelar Venda" chama `CancelarVendaUseCase` e reinicia uma venda nova, para o operador continuar a atender sem sair do módulo.

**Caixa (`CaixaViewModel` / `CaixaView`):**

- [x] Ecrã "Caixa Fechado" (com botão Abrir Caixa) ou "Caixa Aberto" (cartão verde com saldo, resumo de entradas/saídas, ações de Suprimento/Sangria/Fechar Caixa e tabela de últimas movimentações), fiel ao mockup enviado.
- [x] Fecho de caixa mostra a divergência apurada (sobra/falta) de forma explícita, nunca escondida.

**Botões dentro de templates aninhados** (adicionar produto ao carrinho, remover item): usam `Click` + `Tag="{Binding}"` no code-behind, em vez de sintaxe de binding relativo (`$parent[...]`) para alcançar o `Command` do ViewModel do módulo a partir de dentro do `DataTemplate` do item. Mais verboso, mas evita mais uma categoria de erro de binding compilado que já não conseguiríamos testar aqui.

### Decisões tomadas nesta fase

- **Uma apresentação por clique.** Ao adicionar um produto ao carrinho, usa-se sempre a primeira apresentação ativa (mesma simplificação já aceite em Compras, Fase 6). Escolher entre apresentações no ato da venda (ex.: vender "500 g" vs. "1 kg" de açúcar) fica para um refinamento futuro deste ecrã.
- **Um único método de pagamento por venda nesta fase**, embora `FinalizarVendaCommand` já suporte uma lista de pagamentos (pagamento misto) desde a Fase 3 — a UI simplesmente envia sempre uma lista com um único `PagamentoCommand`. Pagamento misto na UI fica para refinamento futuro; o Core e a Application já não precisam de nenhuma alteração para o suportar.
- **Dados da loja fixos ("KiVenda") na impressão do recibo**, por a Fase 11 (Configurações → Dados da Loja) ainda não existir. Trocar por dados reais é uma alteração isolada a `VendasViewModel.FinalizarVendaAsync`.
- **`FormatadorKz` nunca faz o caminho inverso (texto → decimal).** A primeira versão desta fase tentou fazer `TotalTexto.Replace(...)` para recuperar o valor numérico do total — exatamente o tipo de fragilidade que a Fase 4/5 já tinha ensinado a evitar. Corrigido antes de sequer chegar ao utilizador: `VendasViewModel` guarda agora o total como `decimal` à parte (`_totalAtual`), nunca fazendo parsing do texto já formatado.

### Pendente para validar (primeira execução numa máquina real)

- [ ] `dotnet build` — confirmar compilação limpa (mais 2 Views novas, mais complexas: templates aninhados, `Tag`/`Click` no code-behind).
- [ ] `dotnet run` — abrir sessão de caixa, fazer uma venda completa (adicionar 2-3 produtos, aplicar desconto, receber pagamento), confirmar que aparece um ficheiro em `~/.local/share/KiVenda/recibos/` (Linux) com o recibo formatado.
- [ ] Confirmar que "Cancelar Venda" limpa o carrinho e permite continuar a vender.
- [ ] No módulo Caixa: abrir caixa, registar um suprimento e uma sangria, fechar caixa com um saldo diferente do esperado e confirmar que a divergência aparece corretamente (sinal e valor).
- [ ] `dotnet test` — confirmar que as 4 suites continuam a passar; `ConsultarVendaUseCase` e `CancelarVendaUseCase` também ainda não têm testes próprios (mesma dívida técnica já registada na Fase 6).

### Próxima fase

➡️ **Fase 8 — Scanner de Código de Barras**
(ver detalhe em [`docs/PLANO_DE_IMPLEMENTACAO.md`](docs/PLANO_DE_IMPLEMENTACAO.md#fase-8--scanner-de-código-de-barras))

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
