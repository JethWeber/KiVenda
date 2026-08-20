namespace KiVenda.Core.Utilizadores;

/// <summary>
/// Ações do sistema sujeitas a controlo de permissão, conforme a tabela
/// de Perfis de Acesso da documentação funcional (Secção 5).
/// </summary>
public enum Acao
{
    ConfigurarSistema,
    CadastrarProdutos,
    AjustarStock,
    AcederRelatorios,
    CriarUtilizadores,
    RealizarBackup,
    RegistarCompras,
    GerirCaixa,
    FazerVenda,
    ConsultarProdutosStockClientes
}
