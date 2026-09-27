# Ajuste de UI — Central de Cadastros (Clientes / Fornecedores / Funcionários)

Só a UI foi tocada. Nenhuma use case, repositório, migração ou regra de
negócio foi alterada. Substitui os ficheiros abaixo pelos caminhos
correspondentes no teu repositório:

| Ficheiro aqui             | Substitui em                                                              |
|----------------------------|----------------------------------------------------------------------------|
| `CadastrosView.axaml`      | `src/KiVenda.Desktop/Views/Modulos/CadastrosView.axaml`                   |
| `CadastrosViewModel.cs`    | `src/KiVenda.Desktop/ViewModels/Modulos/CadastrosViewModel.cs`            |
| `IniciaisConverter.cs`     | **novo** ficheiro em `src/KiVenda.Desktop/Converters/IniciaisConverter.cs`|
| `App.axaml`                | `src/KiVenda.Desktop/App.axaml` (só adicionei 1 linha a registar o converter novo) |

## O que mudou

- Layout alinhado ao mockup: cabeçalho, pílulas de resumo (Clientes /
  Fornecedores / Funcionários com contagem ao vivo), abas em formato de
  pílula com contador, cartões de indicadores por aba, tabela com
  avatares circulares de iniciais (sem fotos), badges de estado nos
  funcionários e botões de ação em ícone.
- **Nenhum ícone é imagem nem emoji** — são só glifos/símbolos de texto
  (`✓ ✎ × ◈ ▤ ◉ ↗ ▣ ◇ ⌕`), no mesmo estilo que já usavas no Dashboard.
- **Nada de AGT/compliance** entrou no projeto — isso ficou só no Mock,
  como pediste.
- Os indicadores (KPIs) mostrados são só os que dá para calcular a
  partir dos dados que o `CadastrosViewModel` já carrega:
  - **Clientes:** total, com NIF preenchido, com contacto preenchido.
  - **Fornecedores:** total, com NIF preenchido, com produtos fornecidos
    listados.
  - **Funcionários:** total, ativos/inativos, massa salarial (soma dos
    salários base dos ativos), admissões no ano corrente.
  - Tudo o resto do mockup (turnos ao vivo, vendas do mês, prazo médio
    de pagamento, assiduidade, categorias de compra...) não existe nos
    dados do cadastro, por isso não entrou — como pediste.
- `IniciaisConverter.cs` é só um conversor de apresentação (como os que
  já existem em `Converters/`) que tira as iniciais do nome para desenhar
  o avatar circular — não mexe em nenhuma regra de negócio.

## Por rever no repositório

Verifiquei o teu repositório completo (`github.com/JethWeber/KiVenda`) e
o ecrã "Central de Cadastros" (menu **Cadastros**) é mesmo o
`CadastrosViewModel` + `CadastrosView.axaml` — as três abas correspondem
exatamente aos 3 ecrãs do mockup. Não mexi em `ClientesView.axaml`,
`FornecedoresView.axaml` nem `UtilizadoresView.axaml` porque não estão
ligados a nenhum item do menu lateral (não são o ecrã que aparece nas
tuas imagens) — avisa se também queres alinhar esses.
