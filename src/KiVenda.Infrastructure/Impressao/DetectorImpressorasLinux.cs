namespace KiVenda.Infrastructure.Impressao;

public sealed class DetectorImpressorasLinux : IDetectorImpressoras
{
    public Task<IReadOnlyList<DispositivoImpressora>> DetetarAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var caminhos = new HashSet<string>(StringComparer.Ordinal);

        AdicionarSeExistir(caminhos, "/dev/serial/by-id/*");
        AdicionarSeExistir(caminhos, "/dev/usb/lp*");
        AdicionarSeExistir(caminhos, "/dev/ttyUSB*");
        AdicionarSeExistir(caminhos, "/dev/ttyACM*");

        var dispositivos = caminhos
            .Select(CriarDispositivo)
            .OrderBy(d => d.Nome, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<DispositivoImpressora>>(dispositivos);
    }

    private static void AdicionarSeExistir(
        HashSet<string> caminhos,
        string padrao)
    {
        var diretorio = Path.GetDirectoryName(padrao);
        var nome = Path.GetFileName(padrao);

        if (string.IsNullOrWhiteSpace(diretorio) ||
            string.IsNullOrWhiteSpace(nome) ||
            !Directory.Exists(diretorio))
        {
            return;
        }

        foreach (var caminho in Directory.EnumerateFileSystemEntries(diretorio, nome))
        {
            caminhos.Add(caminho);
        }
    }

    private static DispositivoImpressora CriarDispositivo(string caminho)
    {
        var nome = Path.GetFileName(caminho);
        var estado = EstadoDispositivoImpressora.Disponivel;
        string? detalhe = null;

        try
        {
            if (!File.Exists(caminho))
            {
                estado = EstadoDispositivoImpressora.Indisponivel;
                detalhe = "O dispositivo deixou de estar disponível.";
            }
            else
            {
                using var stream = new FileStream(
                    caminho,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.ReadWrite,
                    1,
                    FileOptions.None);
            }
        }
        catch (UnauthorizedAccessException)
        {
            estado = EstadoDispositivoImpressora.SemPermissao;
            detalhe = "Sem permissão de escrita no dispositivo.";
        }
        catch (IOException ex)
        {
            estado = EstadoDispositivoImpressora.Erro;
            detalhe = ex.Message;
        }

        return new DispositivoImpressora(
            caminho,
            nome,
            "Linux",
            caminho,
            estado,
            detalhe);
    }
}
