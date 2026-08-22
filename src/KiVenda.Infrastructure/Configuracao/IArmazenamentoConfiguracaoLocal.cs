namespace KiVenda.Infrastructure.Configuracao;

/// <summary>
/// Armazenamento local de preferências simples (chave → valor), usado
/// por configurações que não pertencem ao domínio de negócio (Core) nem
/// precisam de estar na base de dados relacional — ex.: preferências do
/// scanner, impressora selecionada, tema, idioma (Secção 4.9 /
/// Configurações). Cada implementação decide o formato de persistência;
/// a implementação por defeito usa um único ficheiro JSON local.
/// </summary>
public interface IArmazenamentoConfiguracaoLocal
{
    Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default);

    Task GuardarAsync<T>(string chave, T valor, CancellationToken cancellationToken = default);

    Task RemoverAsync(string chave, CancellationToken cancellationToken = default);
}
