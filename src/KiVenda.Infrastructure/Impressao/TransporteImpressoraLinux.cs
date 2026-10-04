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
                using (var printer = new FilePrinter(configuracao.Dispositivo))
                    printer.Write(dados.ToArray());
                break;

            case TipoConexaoImpressora.Serial:
                using (var printer = new SerialPrinter(configuracao.Dispositivo, configuracao.BaudRate))
                    printer.Write(dados.ToArray());
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
                    $"A conexão '{configuracao.TipoConexao}' não é suportada neste ambiente Linux.");
        }
    }
}
