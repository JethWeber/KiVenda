using System.Text;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Perfil físico/lógico de uma impressora térmica ESC/POS.
/// Não representa uma marca específica; descreve apenas capacidades
/// que podem variar entre modelos.
/// </summary>
public sealed record ConfiguracaoImpressoraTermica(
    string Dispositivo,
    int Colunas = 48,
    int LinhasAlimentacaoFinal = 4,
    bool CortarPapel = true,
    int CodigoPagina = 2,
    string EncodingNome = "cp850")
{
    public Encoding ObterEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(EncodingNome);
    }
}
