using System.IO.Ports;
using ESCPOS_NET;

namespace KiVenda.Infrastructure.Impressao;

public sealed class TransporteImpressoraLinux : ITransporteImpressora
{
    public async Task EnviarAsync(
        ConfiguracaoImpressoraTermica configuracao,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("O transporte Linux só pode ser usado no Linux.");

        configuracao.Validar();

        switch (configuracao.TipoConexao)
        {
            case TipoConexaoImpressora.DispositivoLocal:
                await EnviarFicheiroDispositivoAsync(configuracao.Dispositivo, dados, cancellationToken);
                break;

            case TipoConexaoImpressora.Serial:
                await EnviarSerialAsync(configuracao.Dispositivo, configuracao.BaudRate, dados, cancellationToken);
                break;

            case TipoConexaoImpressora.Rede:
                var printerRede = new ImmediateNetworkPrinter(
                    new ImmediateNetworkPrinterSettings
                    {
                        ConnectionString = $"{configuracao.EnderecoRede}:{configuracao.PortaRede}",
                        PrinterName = configuracao.EnderecoRede,
                        ConnectTimeoutMs = 5000,
                        SendTimeoutMs = 5000
                    });
                await printerRede.WriteAsync(dados.ToArray());
                break;

            default:
                throw new PlatformNotSupportedException(
                    $"A conexão '{configuracao.TipoConexao}' não é suportada no Linux.");
        }
    }

    private static async Task EnviarFicheiroDispositivoAsync(
        string dispositivo,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(dispositivo))
            throw new FileNotFoundException($"O dispositivo '{dispositivo}' não está disponível.", dispositivo);

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
                $"Sem permissão para escrever na impressora '{dispositivo}'. " +
                "No Linux, confirme as permissões do dispositivo e o acesso do utilizador ao grupo adequado.",
                ex);
        }
    }

    private static Task EnviarSerialAsync(
        string porta,
        int baudRate,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var serial = new SerialPort(porta, baudRate)
        {
            WriteTimeout = 5000
        };

        serial.Open();
        var bytes = dados.ToArray();
        serial.Write(bytes, 0, bytes.Length);

        return Task.CompletedTask;
    }
}
