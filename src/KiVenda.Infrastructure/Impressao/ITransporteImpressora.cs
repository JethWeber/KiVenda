namespace KiVenda.Infrastructure.Impressao;

public interface ITransporteImpressora
{
    Task EnviarAsync(
        ConfiguracaoImpressoraTermica configuracao,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken = default);
}
