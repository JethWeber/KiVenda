using System.Text;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Configuração local de uma impressora térmica ESC/POS.
/// É persistida no armazenamento de configurações do KiVenda.
/// </summary>
public sealed record ConfiguracaoImpressoraTermica(
    bool Ativo = false,
    string Dispositivo = "/dev/usb/lp0",
    int Colunas = 48,
    int LinhasAlimentacaoFinal = 4,
    bool CortarPapel = true,
    int CodigoPagina = 2,
    string EncodingNome = "cp850")
{
    public const string Chave = "impressora-termica";

    public static ConfiguracaoImpressoraTermica Padrao => new();

    public Encoding ObterEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(EncodingNome);
    }
}
