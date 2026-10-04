namespace KiVenda.Infrastructure.Impressao;

public interface IServicoImpressaoTermica
{
    Task TestarImpressoraAsync(
        ConfiguracaoImpressoraTermica configuracao,
        CancellationToken cancellationToken = default);
}
