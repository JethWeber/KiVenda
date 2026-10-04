using System.Text;

namespace KiVenda.Infrastructure.Impressao;

public sealed record ConfiguracaoImpressoraTermica(
    TipoConexaoImpressora TipoConexao,
    string Dispositivo = "",
    string EnderecoRede = "",
    int PortaRede = 9100,
    int BaudRate = 115200,
    int Colunas = 48,
    int LinhasAlimentacaoFinal = 4,
    bool CortarPapel = true,
    string EncodingNome = "cp850",
    bool Ativo = false)
{
    public const string Chave = "impressora-termica";

    public static ConfiguracaoImpressoraTermica Padrao =>
        OperatingSystem.IsWindows()
            ? new(TipoConexaoImpressora.WindowsSpooler)
            : new(TipoConexaoImpressora.DispositivoLocal);

    public Encoding ObterEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(EncodingNome);
    }

    public void Validar()
    {
        if (Colunas is < 24 or > 64)
            throw new InvalidOperationException("A largura da impressora deve estar entre 24 e 64 colunas.");

        if (LinhasAlimentacaoFinal is < 0 or > 20)
            throw new InvalidOperationException("As linhas de alimentação final devem estar entre 0 e 20.");

        if (BaudRate is < 1200 or > 921600)
            throw new InvalidOperationException("A velocidade serial não é válida.");

        switch (TipoConexao)
        {
            case TipoConexaoImpressora.WindowsSpooler when string.IsNullOrWhiteSpace(Dispositivo):
                throw new InvalidOperationException("Selecione a impressora instalada no Windows.");
            case TipoConexaoImpressora.DispositivoLocal when string.IsNullOrWhiteSpace(Dispositivo):
                throw new InvalidOperationException("Selecione o dispositivo local da impressora.");
            case TipoConexaoImpressora.Serial when string.IsNullOrWhiteSpace(Dispositivo):
                throw new InvalidOperationException("Indique a porta serial da impressora.");
            case TipoConexaoImpressora.Rede when string.IsNullOrWhiteSpace(EnderecoRede):
                throw new InvalidOperationException("Indique o endereço IP ou nome da impressora de rede.");
            case TipoConexaoImpressora.Rede when PortaRede is < 1 or > 65535:
                throw new InvalidOperationException("A porta TCP da impressora não é válida.");
        }

        _ = ObterEncoding();
    }
}
