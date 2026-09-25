using System.Runtime.InteropServices;

namespace KiVenda.Infrastructure.Impressao;

public sealed class DetectorImpressorasWindows : IDetectorImpressoras
{
    private const uint PrinterEnumLocal = 0x00000002;
    private const uint PrinterEnumConnections = 0x00000004;
    private const int ErrorInsufficientBuffer = 122;

    public Task<IReadOnlyList<DispositivoImpressora>> DetetarAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult<IReadOnlyList<DispositivoImpressora>>(Array.Empty<DispositivoImpressora>());
        }

        var dispositivos = new List<DispositivoImpressora>();
        IntPtr buffer = IntPtr.Zero;

        try
        {
            uint needed = 0;
            uint count = 0;

            EnumPrinters(
                PrinterEnumLocal | PrinterEnumConnections,
                null,
                2,
                IntPtr.Zero,
                0,
                ref needed,
                ref count);

            var erro = Marshal.GetLastWin32Error();
            if (needed == 0 && erro != ErrorInsufficientBuffer)
            {
                return Task.FromResult<IReadOnlyList<DispositivoImpressora>>(dispositivos);
            }

            buffer = Marshal.AllocHGlobal((int)needed);

            if (!EnumPrinters(
                    PrinterEnumLocal | PrinterEnumConnections,
                    null,
                    2,
                    buffer,
                    needed,
                    ref needed,
                    ref count))
            {
                return Task.FromResult<IReadOnlyList<DispositivoImpressora>>(dispositivos);
            }

            var tamanho = Marshal.SizeOf<PrinterInfo2>();

            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var ptr = IntPtr.Add(buffer, i * tamanho);
                var info = Marshal.PtrToStructure<PrinterInfo2>(ptr);

                var nome = LerTexto(info.PrinterName);
                if (string.IsNullOrWhiteSpace(nome))
                {
                    continue;
                }

                var porta = LerTexto(info.PortName);
                var id = $"winspool:{nome}";

                dispositivos.Add(
                    new DispositivoImpressora(
                        id,
                        nome,
                        "Windows",
                        porta,
                        EstadoDispositivoImpressora.Disponivel,
                        string.IsNullOrWhiteSpace(porta) ? null : $"Porta: {porta}"));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            dispositivos.Add(
                new DispositivoImpressora(
                    "windows:erro",
                    "Deteção de impressoras do Windows",
                    "Windows",
                    string.Empty,
                    EstadoDispositivoImpressora.Erro,
                    ex.Message));
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        return Task.FromResult<IReadOnlyList<DispositivoImpressora>>(
            dispositivos
                .OrderBy(d => d.Nome, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private static string LerTexto(IntPtr ponteiro) =>
        ponteiro == IntPtr.Zero
            ? string.Empty
            : Marshal.PtrToStringUni(ponteiro) ?? string.Empty;

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool EnumPrinters(
        uint flags,
        string? name,
        uint level,
        IntPtr printerEnum,
        uint cbBuf,
        ref uint pcbNeeded,
        ref uint pcReturned);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PrinterInfo2
    {
        public IntPtr ServerName;
        public IntPtr PrinterName;
        public IntPtr ShareName;
        public IntPtr PortName;
        public IntPtr DriverName;
        public IntPtr Comment;
        public IntPtr Location;
        public IntPtr DevMode;
        public IntPtr SepFile;
        public IntPtr PrintProcessor;
        public IntPtr DataType;
        public IntPtr Parameters;
        public IntPtr SecurityDescriptor;
        public uint Attributes;
        public uint Priority;
        public uint DefaultPriority;
        public uint StartTime;
        public uint UntilTime;
        public uint Status;
        public uint JobsCount;
        public uint AveragePPM;
    }
}
