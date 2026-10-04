using System.ComponentModel;
using System.Runtime.InteropServices;
using ESCPOS_NET;

namespace KiVenda.Infrastructure.Impressao;

public sealed class TransporteImpressoraWindows : ITransporteImpressora
{
    public async Task EnviarAsync(
        ConfiguracaoImpressoraTermica configuracao,
        ReadOnlyMemory<byte> dados,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("O transporte Windows só pode ser usado no Windows.");

        configuracao.Validar();

        switch (configuracao.TipoConexao)
        {
            case TipoConexaoImpressora.WindowsSpooler:
                EnviarSpooler(configuracao.Dispositivo, dados);
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
                    $"A conexão '{configuracao.TipoConexao}' não é suportada como transporte local no Windows.");
        }
    }

    private static void EnviarSpooler(string dispositivo, ReadOnlyMemory<byte> dados)
    {
        var nomeImpressora = dispositivo.StartsWith("winspool:", StringComparison.OrdinalIgnoreCase)
            ? dispositivo["winspool:".Length..]
            : dispositivo;

        if (string.IsNullOrWhiteSpace(nomeImpressora))
            throw new ArgumentException("O nome da impressora Windows não pode estar vazio.", nameof(dispositivo));

        if (!OpenPrinter(nomeImpressora, out var handle, IntPtr.Zero))
            throw CriarErroWindows($"Não foi possível abrir a impressora '{nomeImpressora}'.");

        try
        {
            var documento = new DocInfo1 { DocName = "KiVenda ESC/POS", DataType = "RAW" };

            if (StartDocPrinter(handle, 1, ref documento) == 0)
                throw CriarErroWindows($"Não foi possível iniciar o trabalho de impressão em '{nomeImpressora}'.");

            try
            {
                if (!StartPagePrinter(handle))
                    throw CriarErroWindows($"Não foi possível iniciar a página em '{nomeImpressora}'.");

                try
                {
                    var bytes = dados.ToArray();
                    var ponteiro = Marshal.AllocHGlobal(bytes.Length);
                    try
                    {
                        Marshal.Copy(bytes, 0, ponteiro, bytes.Length);

                        if (!WritePrinter(handle, ponteiro, bytes.Length, out var escritos) ||
                            escritos != bytes.Length)
                            throw CriarErroWindows($"Falha ao enviar dados ESC/POS para '{nomeImpressora}'.");
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ponteiro);
                    }
                }
                finally
                {
                    EndPagePrinter(handle);
                }
            }
            finally
            {
                EndDocPrinter(handle);
            }
        }
        finally
        {
            ClosePrinter(handle);
        }
    }

    private static Exception CriarErroWindows(string mensagem)
    {
        var erro = Marshal.GetLastWin32Error();
        return new Win32Exception(erro, $"{mensagem} Código Windows: {erro}.");
    }

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool OpenPrinter(string printerName, out IntPtr printer, IntPtr defaults);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr printer);

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint StartDocPrinter(IntPtr printer, uint level, ref DocInfo1 documentInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr printer);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr printer);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr printer);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr printer, IntPtr buffer, int count, out int written);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DocInfo1
    {
        public string? DocName;
        public string? OutputFile;
        public string? DataType;
    }
}
