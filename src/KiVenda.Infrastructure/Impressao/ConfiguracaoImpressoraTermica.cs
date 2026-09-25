using System.Text;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Configuração local de uma impressora térmica ESC/POS.
/// O dispositivo é descoberto automaticamente e persistido como identificador
/// estável do sistema operativo (caminho no Linux ou nome da impressora no Windows).
/// </summary>
public sealed record ConfiguracaoImpressoraTermica(
    string Dispositivo = "",
    int Colunas = 48,
    int LinhasAlimentacaoFinal = 4,
    bool CortarPapel = true,
    int CodigoPagina = 2,
    string EncodingNome = "cp850",
    bool Ativo = false)
{
    public const string Chave = "impressora-termica";

    public static ConfiguracaoImpressoraTermica Padrao => new();

    public Encoding ObterEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(EncodingNome);
    }
}
