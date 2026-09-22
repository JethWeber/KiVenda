using System.Diagnostics;
using System.Text;
using KiVenda.Infrastructure.Configuracao;

namespace KiVenda.Infrastructure.Scanner;

/// <summary>
/// Implementação de <see cref="IServicoScanner"/> para leitores USB tipo
/// teclado (Secção 6). Acumula os caracteres recebidos e mede o
/// intervalo entre teclas consecutivas: um leitor deste tipo envia o
/// código numa rajada muito rápida (tipicamente inferior a
/// <see cref="LimiarMilissegundosEntreTeclas"/> ms entre teclas),
/// terminada por Enter — ao contrário de alguém a digitar manualmente.
///
/// Esta classe não sabe nada sobre UI: recebe caracteres/Enter tal como
/// o Desktop os for repassando a partir do campo de pesquisa do PDV
/// (Fase 8, Parte 3), e só reage de forma diferente quando o padrão de
/// timing bate certo com um scanner.
/// </summary>
public sealed class ServicoScannerTeclado : IServicoScanner
{
    // Intervalo máximo entre teclas consecutivas para ainda serem
    // consideradas parte da mesma rajada de um leitor USB tipo teclado
    // (o plano da Fase 8 estima < 30–50 ms; usamos o limite superior
    // dessa faixa para não rejeitar leitores mais lentos). Qualquer
    // intervalo maior reinicia o buffer — não é tratado como erro, só
    // como "começo de uma sequência nova".
    private const long LimiarMilissegundosEntreTeclas = 50;

    // Um código de barras real tem sempre alguns caracteres; um Enter
    // isolado (buffer vazio ou com 1-2 caracteres) nunca é tratado como
    // leitura de scanner, mesmo que o timing batesse certo — evita
    // falsos positivos em Enters "soltos" do utilizador no campo de
    // pesquisa. Valor de partida conservador; ajustável se algum código
    // de barras real do domínio for mais curto do que isto.
    private const int TamanhoMinimoCodigo = 3;

    private readonly IArmazenamentoConfiguracaoLocal _armazenamentoConfiguracao;
    private readonly Stopwatch _cronometro = Stopwatch.StartNew();
    private readonly StringBuilder _buffer = new();

    private long? _timestampUltimoCaractereMs;

    public ServicoScannerTeclado(IArmazenamentoConfiguracaoLocal armazenamentoConfiguracao)
    {
        _armazenamentoConfiguracao = armazenamentoConfiguracao;

        // Valor por omissão até o primeiro RecarregarConfiguracaoAsync
        // (chamado pelo composition root do Desktop no arranque — ver
        // IServicoScanner.RecarregarConfiguracaoAsync). O construtor não
        // pode ser assíncrono, por isso não lê o ficheiro de configuração
        // aqui.
        ConfiguracaoAtual = ConfiguracaoScanner.Padrao;
    }

    public event Action<string>? CodigoLido;

    public ConfiguracaoScanner ConfiguracaoAtual { get; private set; }

    public void ProcessarCaracter(char caractere)
    {
        if (!ConfiguracaoAtual.Ativo)
        {
            return;
        }

        var agoraMs = _cronometro.ElapsedMilliseconds;

        if (_timestampUltimoCaractereMs is { } ultimoMs &&
            agoraMs - ultimoMs > LimiarMilissegundosEntreTeclas)
        {
            // Intervalo demasiado largo desde o caractere anterior:
            // descarta o que estava acumulado (não era uma rajada de
            // scanner) e começa um buffer novo a partir deste caractere.
            _buffer.Clear();
        }

        _buffer.Append(caractere);
        _timestampUltimoCaractereMs = agoraMs;
    }

    public void ProcessarEnter()
    {
        if (!ConfiguracaoAtual.Ativo)
        {
            Reset();
            return;
        }

        if (_buffer.Length >= TamanhoMinimoCodigo)
        {
            var codigo = _buffer.ToString();
            Reset();
            CodigoLido?.Invoke(codigo);
            return;
        }

        // Buffer vazio ou demasiado curto: não é uma leitura de scanner.
        // Descarta em silêncio — quem trata este Enter como pesquisa
        // manual é a UI (Fase 7), fora deste serviço.
        Reset();
    }

    public void Reset()
    {
        _buffer.Clear();
        _timestampUltimoCaractereMs = null;
    }

    public async Task RecarregarConfiguracaoAsync(CancellationToken cancellationToken = default)
    {
        var configuracao = await _armazenamentoConfiguracao.ObterAsync<ConfiguracaoScanner>(
            ConfiguracaoScanner.Chave, cancellationToken);

        ConfiguracaoAtual = configuracao ?? ConfiguracaoScanner.Padrao;

        // Se a configuração acabou de ser desativada a meio de uma
        // leitura em curso, não deixa um buffer "órfão" pendurado à
        // espera de um Enter que pode nunca reativar o scanner a tempo.
        if (!ConfiguracaoAtual.Ativo)
        {
            Reset();
        }
    }
}
