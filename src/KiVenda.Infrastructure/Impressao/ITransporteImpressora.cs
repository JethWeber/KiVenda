namespace KiVenda.Infrastructure.Impressao;

public interface ITransporteImpressora
{
    Task EnviarAsync(
        string dispositivo,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken = default);
}
