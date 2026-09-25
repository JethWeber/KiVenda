namespace KiVenda.Infrastructure.Impressao;

public sealed class TransporteImpressoraNaoSuportado : ITransporteImpressora
{
    public Task EnviarAsync(
        string dispositivo,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken = default)
    {
        throw new PlatformNotSupportedException(
            $"O sistema operativo atual não possui transporte ESC/POS implementado para '{dispositivo}'.");
    }
}
