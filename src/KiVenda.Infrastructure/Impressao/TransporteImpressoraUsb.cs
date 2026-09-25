namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Transporte USB simples para dispositivos expostos pelo sistema operativo
/// como ficheiro de dispositivo. No Linux, o caso comum de uma impressora
/// térmica USB em modo raw é /dev/usb/lp0 (o caminho é configurável).
/// </summary>
public sealed class TransporteImpressoraUsb
{
    public async Task EnviarAsync(
        string dispositivo,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dispositivo))
            throw new ArgumentException("O caminho do dispositivo USB não pode estar vazio.", nameof(dispositivo));

        if (!File.Exists(dispositivo))
            throw new FileNotFoundException(
                $"A impressora térmica não está disponível no dispositivo '{dispositivo}'.",
                dispositivo);

        await using var stream = new FileStream(
            dispositivo,
            FileMode.Open,
            FileAccess.Write,
            FileShare.ReadWrite,
            bufferSize: 4096,
            options: FileOptions.Asynchronous);

        await stream.WriteAsync(dados, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}
