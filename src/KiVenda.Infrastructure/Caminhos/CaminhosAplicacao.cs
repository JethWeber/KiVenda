namespace KiVenda.Infrastructure.Caminhos;

/// <summary>
/// Ponto único onde a aplicação decide ONDE guarda os seus dados no
/// disco (base de dados, backups, logs, configuração local, licença),
/// de forma consistente em Windows/Linux/macOS. Suporta diretamente o
/// princípio de "instalar e vender em 5 minutos" (Secção 3 da
/// documentação funcional): nenhum destes caminhos exige configuração
/// manual — são criados automaticamente na primeira execução.
/// </summary>
public static class CaminhosAplicacao
{
    private const string NomePasta = "KiVenda";

    /// <summary>
    /// Pasta raiz de dados da aplicação:
    ///   Windows → %APPDATA%\KiVenda
    ///   Linux   → ~/.local/share/KiVenda (via XDG_DATA_HOME quando definido)
    ///   macOS   → ~/Library/Application Support/KiVenda
    /// </summary>
    public static string PastaDados
    {
        get
        {
            var pastaBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            var caminho = Path.Combine(pastaBase, NomePasta);
            Directory.CreateDirectory(caminho);
            return caminho;
        }
    }

    public static string CaminhoBaseDeDados => Path.Combine(PastaDados, "kivenda.db");

    public static string PastaBackups
    {
        get
        {
            var caminho = Path.Combine(PastaDados, "backups");
            Directory.CreateDirectory(caminho);
            return caminho;
        }
    }

    public static string PastaLogs
    {
        get
        {
            var caminho = Path.Combine(PastaDados, "logs");
            Directory.CreateDirectory(caminho);
            return caminho;
        }
    }

    public static string PastaRecibos
    {
        get
        {
            var caminho = Path.Combine(PastaDados, "recibos");
            Directory.CreateDirectory(caminho);
            return caminho;
        }
    }

    public static string CaminhoConfiguracaoLocal => Path.Combine(PastaDados, "configuracao.json");

    public static string CaminhoLicenca => Path.Combine(PastaDados, "licenca.wta");
}
