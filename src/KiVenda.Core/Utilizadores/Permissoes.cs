using KiVenda.Core.Enums;

namespace KiVenda.Core.Utilizadores;

/// <summary>
/// Matriz de permissões por perfil (Secção 5 da documentação funcional).
/// Fonte única de verdade sobre "o que cada perfil pode fazer" — tanto
/// os casos de uso da Application (Fase 3) como a UI (Fase 6, para
/// esconder/desabilitar opções) devem consultar esta classe, em vez de
/// duplicar a regra.
/// </summary>
public static class Permissoes
{
    private static readonly IReadOnlyDictionary<PerfilUtilizador, HashSet<Acao>> PorPerfil = new Dictionary<PerfilUtilizador, HashSet<Acao>>
    {
        [PerfilUtilizador.Gerente] = new HashSet<Acao>
        {
            Acao.ConfigurarSistema,
            Acao.CadastrarProdutos,
            Acao.AjustarStock,
            Acao.AcederRelatorios,
            Acao.CriarUtilizadores,
            Acao.RealizarBackup,
            Acao.RegistarCompras,
            Acao.GerirCaixa,
            Acao.FazerVenda,
            Acao.ConsultarProdutosStockClientes
        },
        [PerfilUtilizador.Atendente] = new HashSet<Acao>
        {
            Acao.FazerVenda,
            Acao.ConsultarProdutosStockClientes
        }
    };

    public static bool Permite(PerfilUtilizador perfil, Acao acao) =>
        PorPerfil.TryGetValue(perfil, out var acoes) && acoes.Contains(acao);
}
