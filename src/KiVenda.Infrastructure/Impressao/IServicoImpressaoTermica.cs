namespace KiVenda.Infrastructure.Impressao;

public interface IServicoImpressaoTermica
{
    Task TestarImpressoraAsync(
        string dispositivo,
        CancellationToken cancellationToken = default);
}
