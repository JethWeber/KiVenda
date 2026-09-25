namespace KiVenda.Infrastructure.Impressao;

public sealed class TransporteImpressoraLinux : ITransporteImpressora
{
    public async Task EnviarAsync(
        string dispositivo,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dispositivo))
        {
            throw new ArgumentException(
                "O dispositivo da impressora não pode estar vazio.",
                nameof(dispositivo));
        }

        if (!File.Exists(dispositivo))
        {
            throw new FileNotFoundException(
                $"A impressora não está disponível no dispositivo '{dispositivo}'.",
                dispositivo);
        }

        try
        {
            await using var stream = new FileStream(
                dispositivo,
                FileMode.Open,
                FileAccess.Write,
                FileShare.ReadWrite,
                4096,
                FileOptions.Asynchronous);

            await stream.WriteAsync(dados, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException(
                $"Sem permissão para escrever na impressora '{dispositivo}'.",
                ex);
        }
    }
}
