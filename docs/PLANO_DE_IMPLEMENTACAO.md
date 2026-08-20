# KiVenda Desktop + Core — Plano de Implementação

**Weber Tech**
**Produto:** KiVenda Desktop (MVP / V1.0)
**Documento:** Plano de Implementação por Fases
**Versão:** 1.1

---

## Registo de Alterações

| Versão | Alteração |
|---|---|
| 1.0 | Versão inicial do plano, fase a fase. |
| 1.1 | **Revisão do domínio de Estoque antes do início da Fase 1.** O modelo passa de `Produto.quantidade` (contador solto) para `Produto → Unidade Base → Apresentações Comerciais → Movimentos de Stock`, com custo por unidade base (custo médio ponderado) e domínio preparado para lote/FIFO no futuro. Ver [Nota de Revisão de Domínio — Estoque](#nota-de-revisão-de-domínio--estoque) e as Fases 1, 2, 3, 6 e 9 atualizadas. |

---

## Sumário

- [Visão Geral do Plano](#visão-geral-do-plano)
- [Arquitetura de Referência](#arquitetura-de-referência)
- [Nota de Revisão de Domínio — Estoque](#nota-de-revisão-de-domínio--estoque)
- [Fase 0 — Fundação do Projeto](#fase-0--fundação-do-projeto)
- [Fase 1 — KiVenda.Core (Entidades e Regras de Negócio)](#fase-1--kivendacore-entidades-e-regras-de-negócio)
- [Fase 2 — KiVenda.Persistence (SQLite + EF Core)](#fase-2--kivendapersistence-sqlite--ef-core)
- [Fase 3 — KiVenda.Application (Casos de Uso)](#fase-3--kivendaapplication-casos-de-uso)
- [Fase 4 — KiVenda.Infrastructure](#fase-4--kivendainfrastructure)
- [Fase 5 — Multiutilizador e Perfis de Acesso](#fase-5--multiutilizador-e-perfis-de-acesso)
- [Fase 6 — Interface Desktop (Avalonia + MVVM): Módulos Base](#fase-6--interface-desktop-avalonia--mvvm-módulos-base)
- [Fase 7 — Módulo de Vendas e Fluxo de Caixa](#fase-7--módulo-de-vendas-e-fluxo-de-caixa)
- [Fase 8 — Scanner de Código de Barras](#fase-8--scanner-de-código-de-barras)
- [Fase 9 — Relatórios](#fase-9--relatórios)
- [Fase 10 — Auditoria (Log de Operações)](#fase-10--auditoria-log-de-operações)
- [Fase 11 — Configurações, Licenciamento e Backup](#fase-11--configurações-licenciamento-e-backup)
- [Fase 12 — Testes](#fase-12--testes)
- [Fase 13 — Empacotamento, Distribuição e Lançamento](#fase-13--empacotamento-distribuição-e-lançamento)
- [Critérios Gerais de Aceitação do MVP](#critérios-gerais-de-aceitação-do-mvp)
- [Fora de Escopo (V1.0)](#fora-de-escopo-v10)

---

## Visão Geral do Plano

Este plano traduz a documentação funcional do KiVenda Desktop num roteiro de implementação técnica, organizado em fases sequenciais e incrementais. Cada fase produz um incremento testável do sistema, respeitando a arquitetura em camadas já definida (Core → Application → Infrastructure → Persistence → Desktop).

Princípios que orientam o plano:

1. **Offline-first**: nenhuma fase depende de servidor externo ou internet.
2. **Core estável primeiro**: regras de negócio e entidades são implementadas e testadas antes da interface gráfica.
3. **Multiutilizador desde a base**: utilizador autenticado e permissões são tratados como conceito transversal, não um adendo tardio.
4. **Cada módulo funcional é entregue de ponta a ponta** (Core → UI) antes de avançar para o seguinte, permitindo demonstrações incrementais ao cliente.
5. **Testabilidade contínua**: cada fase inclui testes automatizados mínimos antes de ser considerada concluída.

---

## Arquitetura de Referência

```
KiVenda.Desktop
├── KiVenda.Core            (Entidades e regras de negócio)
├── KiVenda.Application      (Casos de uso / serviços de aplicação)
├── KiVenda.Infrastructure   (Impressora, scanner, backup, licenciamento)
├── KiVenda.Persistence      (EF Core + SQLite)
└── KiVenda.Desktop          (Avalonia + MVVM — interface gráfica)
```

| Camada | Responsabilidade | Não depende de |
|---|---|---|
| Core | Entidades, Value Objects, regras de negócio puras | Nenhuma outra camada |
| Application | Casos de uso, orquestração, DTOs, validações de fluxo | Infrastructure, Persistence, UI |
| Infrastructure | Impressora, scanner, backup/restore, licenciamento | UI |
| Persistence | EF Core, SQLite, repositórios, migrações | UI |
| Desktop | Avalonia, MVVM, ViewModels, Views | — (camada de topo) |

---

## Nota de Revisão de Domínio — Estoque

> Revisão feita **antes** do início da codificação da Fase 1, a pedido de Jeth Weber, por o modelo original de stock (`Produto.quantidade`, contador solto) ser demasiado simples para o público-alvo real do KiVenda (mercearias, cantinas, papelarias, lojas de material de construção, cosméticos, supermercados pequenos), onde o mesmo produto é comprado numa unidade e vendido noutra (ex.: açúcar comprado em saco de 25 kg e vendido a 500 g).

### Problema do modelo original

```
Produto (nome, código, categoria, preço compra, preço venda, quantidade, stock mínimo)
```

Este modelo assume implicitamente que:
- a unidade em que se compra é a mesma em que se vende;
- o `quantidade` é um número solto, sem histórico nem rastreabilidade;
- o `PrecoCompra` é um valor fixo, que não reflete variações de custo entre compras sucessivas.

Nenhuma destas três suposições é verdadeira para o público-alvo do KiVenda.

### Modelo revisto

```
Produto
│
├── UnidadeBase            (ex.: grama, mililitro, unidade)
│
├── Apresentações          (formas de comprar/vender o produto)
│    ├── Unidade      → 1 unidade
│    ├── Caixinha     → 50 unidades
│    └── Caixa Grande → 1.250 unidades
│
└── MovimentosStock        (fonte de verdade do estoque, não um contador solto)
     ├── Entrada (compra)
     ├── Saída (venda)
     └── Ajuste (correção manual)
```

**Exemplos de referência:**

| Produto | Unidade base | Apresentações |
|---|---|---|
| Lapiseira | unidade | Unidade (1), Caixinha (50), Caixa Grande (1.250) |
| Açúcar | grama (g) | 250 g, 500 g, 1 kg (1.000 g), 25 kg (25.000 g) |
| Arroz | grama (g) | 500 g, 1 kg (1.000 g), 5 kg (5.000 g), 25 kg (25.000 g) |

### Princípios adotados a partir desta revisão

1. **Unidade de estoque ≠ unidade de venda.** O estoque é sempre guardado e movimentado na unidade base do produto (ex.: gramas). As apresentações comerciais (250 g, 1 kg, 25 kg) são apenas formas de comprar/vender que se convertem para a unidade base através de um fator de conversão.
2. **O stock é derivado de movimentos, não é um campo solto.** `Produto.EstoqueAtual` passa a existir apenas como **valor materializado** (para leitura rápida no Dashboard/PDV), mas a **fonte de verdade** é sempre o histórico de `MovimentoStock` (Entrada/Saída/Ajuste). Qualquer divergência deve poder ser explicada recalculando os movimentos.
3. **Custo por unidade base, não preço de compra fixo.** Cada `MovimentoStock` de entrada guarda o custo total e a quantidade na unidade base, permitindo calcular o custo por unidade base daquela entrada (ex.: 25.000 Kz / 25.000 g = 1 Kz/g). O custo do produto para efeitos de lucro passa a ser o **custo médio ponderado** de todas as entradas ainda em stock, recalculado a cada nova entrada — e não um valor fixo editado manualmente.
4. **Domínio preparado para lote, sem o implementar no MVP.** As entidades são desenhadas para que um `MovimentoStock` de entrada possa, no futuro, referenciar um `Lote` (para suportar FIFO ou custo por lote), sem exigir remodelação do Core. No V1, a política de custeio adotada é o **custo médio ponderado**, por ser suficiente e mais simples de operar para o público-alvo.
5. **Nenhuma reescrita do Core esperada quando aparecer o primeiro cliente "híbrido"** (ex.: uma loja que vende arroz solto a 500 g e também em sacos fechados de 25 kg) — este é precisamente o cenário que o modelo revisto já cobre desde a Fase 1.

Este ajuste está refletido nas Fases 1 (entidades), 2 (persistência), 3 (casos de uso), 6.3 (UI de Produtos), 7.1 (UI de Vendas) e 9 (Relatórios) abaixo.

---

## Fase 0 — Fundação do Projeto

**Objetivo:** preparar a base técnica sobre a qual todas as fases seguintes serão construídas.

- Criar a solução `.sln` e os 5 projetos da arquitetura (Core, Application, Infrastructure, Persistence, Desktop).
- Configurar referências entre camadas respeitando a direção de dependência (Desktop → Application → Core; Persistence/Infrastructure → Core).
- Configurar o projeto Avalonia com o padrão MVVM (ex.: CommunityToolkit.Mvvm ou ReactiveUI).
- Definir convenções de código, estrutura de pastas e padrão de nomenclatura.
- Configurar repositório Git, `.gitignore`, branch strategy (ex.: `main` + `develop` + feature branches).
- Configurar pipeline básico de build local (dotnet build/test).
- Definir estratégia de injeção de dependência (Microsoft.Extensions.DependencyInjection).
- Configurar logging básico (ex.: Serilog) para diagnóstico local.

**Entregável:** solução compilável, "Hello World" da janela principal do Avalonia a abrir.

---

## Fase 1 — KiVenda.Core (Entidades e Regras de Negócio)

**Objetivo:** modelar o domínio do negócio de forma independente de qualquer tecnologia de UI ou base de dados.

> ⚠️ Ver [Nota de Revisão de Domínio — Estoque](#nota-de-revisão-de-domínio--estoque) antes de implementar esta fase: o modelo de stock abaixo já reflete a revisão (Unidade Base + Apresentações + Movimentos), substituindo o `Produto.quantidade` solto da versão 1.0 do plano.

### 1.1 Entidades de Produto e Estoque (núcleo revisto)

- `Produto` (nome, código interno, código de barras opcional, categoria, `UnidadeBaseId`, preço de venda por unidade base, stock mínimo em unidade base, foto opcional). **Deixa de ter `PrecoCompra` e `Quantidade` como campos soltos.**
- `UnidadeMedida` (ex.: `un`, `g`, `ml`) — a unidade indivisível em que o stock é sempre guardado e movimentado.
- `ApresentacaoProduto` (produto, nome comercial — ex.: "Saco 25kg", "Caixinha", "Unidade" —, fator de conversão para a unidade base, código de barras próprio quando aplicável). Um produto tem sempre pelo menos uma apresentação (a "unidade base" como apresentação default).
- `MovimentoStock` (produto, tipo: Entrada/Saída/Ajuste, quantidade em unidade base, custo unitário na unidade base quando aplicável, referência à origem — `Compra`, `Venda` ou ajuste manual —, utilizador responsável, data/hora). **Fonte de verdade do estoque.**
- `Produto.EstoqueAtual` mantido como **valor materializado** (campo calculado, atualizado a cada `MovimentoStock`) apenas para leitura rápida — nunca escrito diretamente fora desse mecanismo.
- `Lote` — entidade **desenhada mas não usada operacionalmente no MVP**: `MovimentoStock` de entrada já referencia um `LoteId` opcional, deixando o domínio pronto para custeio por lote (FIFO) numa fase futura sem remodelação do Core.
- `Categoria`

### 1.2 Restantes entidades

- `Cliente` (nome, telefone, histórico de compras)
- `Fornecedor` (nome, telefone, produtos fornecidos)
- `Compra` e `ItemCompra` (fornecedor, data, produto, apresentação comprada, quantidade na apresentação, custo total do item — a partir do qual se deriva o custo por unidade base)
- `Venda` e `ItemVenda` (produto, apresentação vendida, quantidade na apresentação, preço, utilizador responsável, cliente opcional)
- `Pagamento` (método: Dinheiro, MCX/Multicaixa, TPA)
- `SessaoCaixa` (abertura, fecho, saldo inicial, saldo final calculado, utilizador)
- `MovimentoCaixa` (Suprimento/Entrada, Sangria/Saída, associado a uma venda ou operação manual)
- `Utilizador` e `Perfil` (Gerente, Atendente, Caixa)
- `LogAuditoria` (utilizador, ação, data/hora, entidade afetada)

### 1.3 Regras de negócio a implementar nesta fase (sem dependência de banco)

- **Conversão de quantidade:** dado uma `ApresentacaoProduto` e uma quantidade nessa apresentação, calcular a quantidade equivalente em unidade base (e o inverso, quando necessário para exibição).
- **Registo de `MovimentoStock` a partir de compra:** dá entrada em unidade base e regista o custo por unidade base dessa entrada (custo total do item ÷ quantidade em unidade base).
- **Cálculo de custo médio ponderado do produto:** recalculado a cada nova entrada de stock, a partir do histórico de `MovimentoStock` de entrada ainda "em stock" (quantidade recebida − já consumida).
- **Registo de `MovimentoStock` a partir de venda:** dá saída em unidade base (convertida a partir da apresentação vendida) e não permite stock negativo.
- **Cálculo de `EstoqueAtual` materializado:** soma de entradas − saídas − ajustes negativos + ajustes positivos, sempre em unidade base; deve poder ser **recalculado a partir do histórico** para fins de auditoria/divergência.
- **Cálculo de lucro estimado:** `preço de venda − custo médio ponderado por unidade base`, aplicado à quantidade em unidade base da venda (não mais `preço venda − PrecoCompra fixo`).
- **Regras de estado de stock** (Em Stock / Baixo Stock / Sem Stock) a partir do stock mínimo, comparado sempre em unidade base.
- **Regras de cálculo de saldo de caixa** (entradas − saídas + saldo inicial).
- **Regras de permissão por perfil** (o que cada perfil pode/não pode fazer), mapeadas conforme a tabela funcional do documento.
- **Validações de domínio:** preço não pode ser negativo; fator de conversão de uma apresentação tem de ser positivo; quantidade em unidade base não pode ficar negativa; um produto não pode ser eliminado se tiver movimentos de stock associados (apenas inativado).

**Entregável:** biblioteca `KiVenda.Core` com entidades (incluindo `UnidadeMedida`, `ApresentacaoProduto`, `MovimentoStock`, `Lote` preparado), enums e regras de negócio — incluindo conversão de unidades e custo médio ponderado — cobertas por testes unitários.

---

## Fase 2 — KiVenda.Persistence (SQLite + EF Core)

**Objetivo:** persistir as entidades do Core localmente, sem qualquer servidor externo.

- Configurar `DbContext` com EF Core apontando para SQLite.
- Mapear entidades (Fluent API), incluindo relações (Venda → ItemVenda, Compra → ItemCompra, Utilizador → Vendas, `Produto` → `ApresentacaoProduto` → `MovimentoStock`, `MovimentoStock` → `Lote` opcional, etc.).
- Índices dedicados em `MovimentoStock` por `ProdutoId` + data, para que o recálculo do custo médio ponderado e do estoque materializado seja rápido mesmo com histórico extenso.
- Criar migrações iniciais (`InitialCreate`), já incluindo `UnidadeMedida`, `ApresentacaoProduto`, `MovimentoStock` e `Lote` desde o primeiro schema (evita uma migração de remodelação de stock mais tarde).
- Implementar repositórios (Produto, ApresentacaoProduto, MovimentoStock, Cliente, Fornecedor, Venda, Compra, Caixa, Utilizador, Auditoria) atrás de interfaces definidas no Core/Application.
- Implementar `UnitOfWork` para garantir transações consistentes (ex.: venda + `MovimentoStock` de saída + atualização do `EstoqueAtual` materializado + movimento de caixa, tudo numa única transação).
- Configurar geração automática do ficheiro `.db` local na primeira execução (auto-provisioning), alinhado ao princípio "instalar e vender em 5 minutos".
- Implementar seed inicial (utilizador Gerente padrão, categorias básicas, unidades de medida padrão — `un`, `g`, `ml`) para primeira execução.

**Entregável:** persistência funcional local, testável com testes de integração usando SQLite em memória/ficheiro temporário — incluindo um teste que recalcula o estoque de um produto a partir do zero, somando os `MovimentoStock`, e confirma que bate com o valor materializado.

---

## Fase 3 — KiVenda.Application (Casos de Uso)

**Objetivo:** orquestrar os casos de uso do sistema, isolando a UI das regras de negócio e do acesso a dados.

Casos de uso a implementar por módulo:

- **Produtos:** CriarProduto, EditarProduto, InativarProduto (substitui "EliminarProduto" — um produto com movimentos de stock não pode ser apagado, só inativado), CriarApresentacaoProduto, EditarApresentacaoProduto, ListarProdutos (com filtros por categoria/estado/pesquisa).
- **Stock** (novo agrupamento de casos de uso, dedicado, em vez de ficar solto em Produtos):
  - `RegistarEntradaStock` — usado internamente por `RegistarCompra` e diretamente para entradas avulsas.
  - `RegistarSaidaStock` — usado internamente por `FinalizarVenda`, recebendo apresentação + quantidade e convertendo para unidade base.
  - `RegistarAjusteStock` — correções manuais (quebras, contagem física), sempre com motivo obrigatório.
  - `ConsultarStock` — devolve o estoque atual (materializado) de um produto em unidade base e, opcionalmente, convertido para uma apresentação à escolha.
  - `ConsultarMovimentosStock` — histórico paginado de `MovimentoStock` por produto/período, usado para auditoria de divergências.
  - `RecalcularEstoqueMaterializado` — reprocessa os `MovimentoStock` de um produto e corrige o valor materializado, para diagnóstico de divergências.
- **Compras:** RegistarCompra (recebe produto + apresentação comprada + quantidade + custo total; deriva o custo por unidade base e chama `RegistarEntradaStock`, atualizando o custo médio ponderado do produto), ListarCompras.
- **Vendas:** IniciarVenda, AdicionarItem (recebe produto + apresentação + quantidade), RemoverItem, AplicarDesconto, FinalizarVenda (recebe pagamento, emite recibo, chama `RegistarSaidaStock` por item, calcula lucro do item a partir do custo médio ponderado, associa utilizador).
- **Caixa:** AbrirCaixa, FecharCaixa (com cálculo de divergência), RegistarSuprimento, RegistarSangria, ConsultarMovimentações.
- **Clientes:** CriarCliente, EditarCliente, ConsultarHistoricoCompras.
- **Fornecedores:** CriarFornecedor, EditarFornecedor.
- **Relatórios:** GerarRelatorioDiario, GerarRelatorioMensal, GerarRelatorioStock (lucro calculado a partir do custo médio ponderado por unidade base, nunca de um preço de compra fixo).
- **Utilizadores:** CriarUtilizador, DefinirPerfil, AutenticarUtilizador (login local), AlterarPassword.
- **Auditoria:** RegistarEventoAuditoria (invocado internamente por outros casos de uso sensíveis, incluindo `RegistarAjusteStock`).

Cada caso de uso deve:

- Validar permissões do perfil do utilizador autenticado.
- Validar regras de negócio antes de persistir.
- Retornar DTOs desacoplados das entidades do Core.
- Disparar registo de auditoria quando aplicável.

**Entregável:** camada de aplicação testável isoladamente (testes com repositórios em memória/mocks).

---

## Fase 4 — KiVenda.Infrastructure

**Objetivo:** implementar integrações técnicas que dependem do ambiente Desktop.

- **Impressora:** serviço de impressão de recibos (térmica ou padrão), com template simples (dados da loja, itens, total, forma de pagamento).
- **Backup/Restore:** serviço de exportação do ficheiro SQLite (ou dump) para local escolhido pelo utilizador (pen drive, pasta local) e respetiva restauração.
- **Licenciamento:** integração com o sistema de licenciamento corporativo da Weber Tech (par de chaves pública/privada, leitura e validação do ficheiro `.wta`), incluindo verificação na inicialização da aplicação.
- **Scanner de código de barras:** camada de escuta de input tipo teclado (ver Fase 8 para detalhe funcional).

**Entregável:** serviços de infraestrutura isolados por interface, prontos para serem consumidos pela Application/Desktop.

---

## Fase 5 — Multiutilizador e Perfis de Acesso

**Objetivo:** implementar o mecanismo transversal de autenticação e permissões antes de construir as telas finais, já que toda a navegação e ação dependerá disto.

- Tela de login local (utilizador + password), sem dependência de internet.
- Sessão de utilizador ativa em memória durante a execução da aplicação.
- Mapeamento de perfis conforme tabela funcional:

| Ação | Gerente | Atendente |
|---|---|---|
| Configurar sistema | ✔ | ✗ |
| Cadastrar produtos | ✔ | ✗ |
| Ajustar stock | ✔ | ✗ |
| Aceder a relatórios | ✔ | ✗ |
| Criar utilizadores | ✔ | ✗ |
| Realizar backup | ✔ | ✗ |
| Registar compras | ✔ | ✗ |
| Gerir caixa | ✔ | ✗ |
| Fazer venda | ✔ | ✔ |
| Consultar produtos/stock/clientes | ✔ | ✔ |

- Implementar guarda de permissões reutilizável (ex.: atributo/decorator ou verificação central na Application) para bloquear ações não autorizadas mesmo que a UI tente escondê-las.
- Ocultar/desabilitar na UI opções não permitidas ao perfil Atendente.
- Associar toda venda e movimento de caixa ao utilizador autenticado.

**Entregável:** sistema de login funcional, com dois perfis distintos validados de ponta a ponta (regra de negócio + UI).

---

## Fase 6 — Interface Desktop (Avalonia + MVVM): Módulos Base

**Objetivo:** construir a shell da aplicação e os módulos de cadastro que servem de base para Vendas e Caixa.

### 6.1 Shell da Aplicação
- Menu lateral fixo com os 10 módulos: Dashboard, Vendas, Produtos, Compras, Clientes, Fornecedores, Caixa, Relatórios, Utilizadores, Configurações.
- Barra superior com pesquisa global, notificações e utilizador autenticado.
- Navegação reativa ao perfil (itens ocultos/desabilitados conforme permissão).

### 6.2 Dashboard
- Indicadores: Vendas de Hoje, Caixa Atual, Lucro Estimado, Stock Baixo, Vendas Realizadas.
- Gráfico simples de tendência de vendas (7/30 dias).
- Painel de alertas (stock crítico, backup pendente, sessão de caixa aberta há muito tempo).

### 6.3 Produtos
- Listagem com pesquisa por nome/código/EAN, filtro por categoria e por estado (Todos/Em Stock/Baixo). Stock exibido sempre convertido para uma unidade legível (ex.: "23,3 kg" em vez de "23.300 g").
- Cadastro do produto: nome, código, categoria, unidade base (`un`, `g`, `ml`), preço de venda por unidade base, stock mínimo, código de barras opcional, foto opcional.
- **Gestão de apresentações comerciais** do produto, dentro do próprio cadastro: adicionar/editar/remover apresentações (ex.: "250 g", "1 kg", "25 kg"), cada uma com o seu fator de conversão para a unidade base e, opcionalmente, o seu próprio código de barras (para produtos vendidos em embalagem fechada).
- Ações: Novo Produto, Editar, Inativar (substitui "Eliminar" quando o produto já tem movimentos de stock), Entrada de Stock, Ajuste de Stock — as duas últimas via os casos de uso `RegistarEntradaStock`/`RegistarAjusteStock` (Fase 3), nunca escrevendo diretamente no stock.
- Ecrã/aba de **Movimentos de Stock** por produto: histórico paginado (Entrada/Saída/Ajuste, quantidade em unidade base, custo quando aplicável, origem, utilizador, data) — usa `ConsultarMovimentosStock`.
- Indicadores de rodapé: total em stock (em unidade base, com conversão amigável), valor do inventário (a custo médio ponderado), produtos com stock baixo, categorias ativas.

### 6.4 Compras
- Registo de compra com seleção de fornecedor e, por item, produto + **apresentação comprada** (ex.: "Saco 25 kg") + quantidade nessa apresentação + custo total do item.
- Exibição do custo por unidade base calculado automaticamente (ex.: "1,10 Kz/g"), antes de confirmar, para o operador validar o valor.
- Ao confirmar, disparo automático de `RegistarEntradaStock` (via caso de uso da Fase 3), que atualiza o custo médio ponderado do produto.
- Listagem histórica de compras.

### 6.5 Clientes
- Cadastro simples (nome, telefone).
- Histórico de compras associado ao cliente (sem controlo de dívida/fiado — fora do MVP).

### 6.6 Fornecedores
- Cadastro (nome, telefone, produtos fornecidos), usado para agilizar o registo de compras.

### 6.7 Utilizadores (visão de gestão)
- Listagem de utilizadores, criação, definição de perfil (Gerente/Atendente).
- Restrito ao perfil Gerente.

**Entregável:** módulos de cadastro navegáveis e funcionais, integrados com Application/Persistence, respeitando permissões por perfil.

---

## Fase 7 — Módulo de Vendas e Fluxo de Caixa

**Objetivo:** implementar o módulo central do sistema, de ponta a ponta.

### 7.1 Tela de Vendas (PDV)
- Grelha de produtos com pesquisa/filtro por categoria e campo dedicado para pesquisar/bipar código.
- **Seleção de apresentação ao adicionar o produto ao carrinho**: quando o produto tem mais do que uma apresentação (ex.: açúcar em 250 g / 1 kg / 25 kg), o PDV pergunta qual está a ser vendida antes de adicionar; quando só existe uma apresentação (caso comum de produtos vendidos por unidade), o passo é automático e não interrompe o fluxo.
- Se o código bipado corresponder ao código de barras de uma **apresentação específica** (ex.: o saco fechado de 1 kg tem o seu próprio EAN), a apresentação é preenchida automaticamente sem necessidade de seleção manual.
- Carrinho lateral com itens (produto + apresentação vendida), quantidades, subtotal, desconto e total.
- Atalhos de teclado (ex.: F2 Nova Venda, F10 Receber, F1 foco na pesquisa).
- Fluxo: Selecionar produto → (Selecionar apresentação, se aplicável) → Definir quantidade → Receber pagamento → Emitir recibo → dar saída de stock em unidade base via `RegistarSaidaStock`.
- Seleção de método de pagamento (Dinheiro, MCX, TPA), com possibilidade de pagamento misto (avaliar no detalhamento técnico).
- Emissão de recibo via serviço de impressão (Fase 4), com a apresentação vendida impressa de forma legível (ex.: "Açúcar 1 kg", não "Açúcar 1.000 g").
- Cancelamento de venda em curso.

### 7.2 Módulo de Caixa
- Ecrã "Caixa Aberto"/"Caixa Fechado" com saldo atual, hora de abertura e operador responsável.
- Resumo rápido: total de entradas, total de saídas, quebra por método de pagamento.
- Ações de Suprimento (entrada manual de valor) e Sangria (saída manual de valor).
- Últimas movimentações (histórico paginado do turno atual), com tipo (Entrada/Saída), operador, valor e ações.
- Fecho de Caixa: cálculo do saldo esperado vs. saldo informado, identificação de divergências, encerramento do turno.
- Toda venda finalizada gera automaticamente um movimento de entrada no caixa da sessão ativa.

**Entregável:** fluxo de venda completo e fluxo de caixa completo (abertura → operação → fecho), validados com cenários reais (venda simples, venda com múltiplos métodos, suprimento, sangria, fecho com divergência).

---

## Fase 8 — Scanner de Código de Barras

**Objetivo:** suportar leitores USB (tipo teclado) sem necessidade de driver dedicado.

- Implementar escuta no campo de pesquisa de produto da tela de Vendas, capturando sequência de dígitos finalizada por Enter.
- Ao detetar leitura válida, localizar produto por código de barras e adicionar automaticamente ao carrinho (ou abrir campo de quantidade, conforme configuração).
- Adicionar campo "Código de Barras" (opcional) ao cadastro de Produto.
- Garantir funcionamento normal por código interno quando o produto não tiver código de barras.
- Implementar tela de Configurações do Scanner: ativar/desativar, emitir som ao ler, adicionar automaticamente, abrir quantidade após leitura.
- Testar com leitor físico real e com simulação de input de teclado.

**Entregável:** fluxo de venda "Bipar → Produto aparece → Quantidade ++ → Próximo produto" funcional e configurável.

---

## Fase 9 — Relatórios

**Objetivo:** entregar apenas os relatórios essenciais definidos no escopo, sem excesso de gráficos.

- **Relatório Diário:** total vendido, lucro, produtos vendidos.
- **Relatório Mensal:** receita, lucro, produtos mais vendidos.
- **Relatório de Stock:** produtos em falta, produtos com stock baixo — quantidades exibidas em unidade base convertida para uma apresentação legível.
- **Lucro sempre calculado a partir do custo médio ponderado por unidade base** (Fase 1), nunca de um `PrecoCompra` fixo — importante para produtos cujo custo variou entre compras (ex.: açúcar comprado a 1 Kz/g numa compra e 1,10 Kz/g na seguinte).
- Filtro por período e, quando aplicável, por utilizador (ex.: "vendas da Maria").
- Exportação simples (ex.: PDF ou impressão) dos relatórios, reaproveitando o serviço de impressão da Fase 4.
- Restrito ao perfil Gerente.

**Entregável:** três relatórios funcionais, com dados corretos validados contra a base de dados de teste.

---

## Fase 10 — Auditoria (Log de Operações)

**Objetivo:** registar operações sensíveis para rastreabilidade e proteção do gerente.

- Definir lista de eventos auditáveis: venda realizada, alteração de preço, eliminação de produto, criação/edição de utilizador, ajuste manual de stock, abertura/fecho de caixa, suprimento/sangria, restauração de backup.
- Implementar `LogAuditoria` (utilizador, ação, entidade afetada, data/hora, valores antes/depois quando relevante).
- Persistir automaticamente a partir dos casos de uso da Application (interceptação central, evitando duplicação de código em cada caso de uso).
- Disponibilizar consulta interna (mesmo que não esteja em destaque na navegação principal), acessível ao perfil Gerente, com filtro por utilizador/período/tipo de ação.

**Entregável:** trilha de auditoria funcional, testada com cenários de divergência de caixa e alteração indevida de dados.

---

## Fase 11 — Configurações, Licenciamento e Backup

**Objetivo:** finalizar os módulos de suporte administrativo do sistema.

- **Dados da Loja:** nome, endereço, contacto, logótipo (usado no recibo e nos relatórios).
- **Impressora:** seleção e teste de impressora.
- **Backup / Restaurar Backup:** interface para acionar o serviço da Fase 4, com validação de integridade do ficheiro restaurado.
- **Licença:** ativação da licença (`.wta`), estado atual, validade.
- **Utilizadores:** gestão completa (ligado à Fase 5).
- **Scanner de Código de Barras:** configurações (ligado à Fase 8).
- **Base de Dados:** localização do ficheiro SQLite, informações de tamanho/uso.
- **Idioma e Tema:** preferências de interface.
- **Sobre:** versão da aplicação, suporte, informações da Weber Tech.
- Implementar o fluxo de primeira execução: instalar → ativar licença → criar utilizador Gerente → começar a vender, garantindo o objetivo de "menos de 5 minutos" descrito na documentação.

**Entregável:** módulo de Configurações completo e fluxo de onboarding validado do zero (instalação limpa).

---

## Fase 12 — Testes

**Objetivo:** garantir qualidade e estabilidade antes do lançamento, cobrindo todas as camadas.

### 12.1 Testes Unitários (Core e Application)
- Regras de cálculo de lucro, stock, saldo de caixa.
- Regras de permissão por perfil.
- Casos de uso com repositórios mockados (cenários de sucesso e de erro/validação).

### 12.2 Testes de Integração (Persistence)
- Migrações aplicadas corretamente em base SQLite limpa.
- Transações compostas (ex.: venda → baixa de stock → movimento de caixa) com rollback em caso de falha.
- Seed inicial correto na primeira execução.

### 12.3 Testes de Infraestrutura
- Impressão de recibo (mock de impressora e, quando possível, teste com impressora física).
- Backup e restauração (round-trip: backup → corromper/apagar base → restaurar → validar integridade).
- Validação de licença (`.wta` válido, inválido, expirado).
- Scanner: simulação de input tipo teclado e leitura de código real.

### 12.4 Testes de Interface (UI/UX)
- Fluxo completo de venda (com e sem scanner).
- Fluxo completo de caixa (abertura, suprimento, sangria, fecho com e sem divergência).
- Visibilidade e bloqueio de ações por perfil (Gerente vs. Atendente).
- Responsividade e usabilidade da navegação principal.

### 12.5 Testes de Aceitação (com cliente piloto)
- Execução do fluxo real de um dia de operação numa loja piloto (cantina/mini mercado).
- Validação do tempo de onboarding (instalação até primeira venda).
- Recolha de feedback sobre fluxo de vendas com scanner e sem scanner.
- Validação dos relatórios diário/mensal contra a contagem manual do comerciante.

### 12.6 Testes de Regressão e Não-Funcionais
- Desempenho com volume realista de dados (ex.: milhares de produtos e vendas).
- Comportamento 100% offline confirmado (sem qualquer chamada de rede).
- Testes de instalação em máquinas limpas (Windows, principal alvo do público).

**Entregável:** suite de testes automatizados (unitários + integração) em execução contínua, relatório de testes manuais de aceitação assinado com o cliente piloto, e lista de bugs conhecidos triada por severidade.

---

## Fase 13 — Empacotamento, Distribuição e Lançamento

**Objetivo:** preparar o produto final para chegar ao comerciante.

- Gerar instalador Desktop (ex.: MSIX/Setup) com processo de instalação simples e guiado.
- Empacotar SQLite e dependências do Avalonia de forma autossuficiente (sem exigir instalação manual de runtime, quando possível).
- Integrar fluxo de ativação de licença ao instalador/primeira execução.
- Preparar material de suporte: guia rápido de instalação, guia rápido de uso do PDV, FAQ de backup/restauração.
- Definir processo de atualização de versão (manual, dado o caráter offline do produto).
- Publicar checklist de lançamento (go-live) e plano de suporte pós-lançamento para o cliente piloto.

**Entregável:** instalador pronto para distribuição, documentação de suporte entregue, cliente piloto operando em produção.

---

## Critérios Gerais de Aceitação do MVP

O MVP do KiVenda Desktop é considerado concluído quando, cumulativamente:

- [ ] Os 10 módulos do menu principal estão implementados e funcionais.
- [ ] O sistema funciona 100% offline, sem qualquer dependência de rede.
- [ ] O fluxo de instalação até à primeira venda é concluído em menos de 5 minutos.
- [ ] O multiutilizador está ativo, com perfis Gerente e Atendente corretamente restringidos.
- [ ] Toda venda e movimento de caixa fica associado ao utilizador que a realizou.
- [ ] O scanner de código de barras funciona sem driver dedicado, com as configurações previstas.
- [ ] A auditoria regista corretamente as operações sensíveis definidas.
- [ ] Os relatórios diário, mensal e de stock refletem corretamente os dados reais.
- [ ] Backup e restauração funcionam de forma confiável (round-trip validado).
- [ ] A licença é validada corretamente na inicialização.
- [ ] A suite de testes (unitários, integração e aceitação) passa sem falhas críticas.
- [ ] **Um produto pode ser cadastrado com mais do que uma apresentação comercial** (ex.: açúcar em 250 g, 1 kg e 25 kg) e vendido/comprado em qualquer uma delas, com o stock sempre coerente em unidade base.
- [ ] **O estoque atual de qualquer produto pode ser recalculado a partir do zero**, somando o histórico de `MovimentoStock`, e bate com o valor materializado exibido na UI.
- [ ] **O lucro reportado usa sempre o custo médio ponderado por unidade base**, refletindo corretamente cenários em que o mesmo produto foi comprado a custos diferentes em compras sucessivas.

---

## Fora de Escopo (V1.0)

Conforme definido na documentação funcional, permanecem fora deste plano de implementação, para versões futuras:

- Kilape (controlo de dívida/fiado)
- Inteligência Artificial
- Catálogo Público
- Integração com WhatsApp
- Encomendas Online
- Sincronização com a nuvem
- Aplicação Mobile
- Dashboard avançado (múltiplos gráficos)
- Fidelização de clientes / Programa de pontos
- **Custeio por lote (FIFO) e política de custeio configurável** — o domínio já prevê `Lote` como referência opcional em `MovimentoStock` (Fase 1), mas no V1 a única política ativa é o custo médio ponderado; ativar FIFO/lote passa a ser apenas uma extensão dos casos de uso existentes, não uma remodelação do Core.

Estas funcionalidades deverão ser reavaliadas nas fases seguintes da visão estratégica do produto (KiVenda PWA → SaaS → Mobile), aproveitando as camadas Core e Application já validadas nesta implementação.
