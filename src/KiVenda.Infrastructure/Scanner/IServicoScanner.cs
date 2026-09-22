using KiVenda.Infrastructure.Configuracao;

namespace KiVenda.Infrastructure.Scanner;

/// <summary>
/// Distingue uma leitura de scanner de código de barras (periférico tipo
/// teclado, Secção 6 da documentação funcional) de digitação humana
/// normal, e expõe o resultado como um evento simples — para o Desktop
/// (Fase 8, Parte 3) não precisar de saber nada sobre timing de teclas.
///
/// O scanner físico não tem driver próprio: envia os caracteres do
/// código como se fossem teclas normais, seguidas de Enter, numa rajada
/// muito mais rápida do que a de uma pessoa a digitar. Esta interface
/// recebe esses caracteres um a um, tal como chegam ao campo de
/// pesquisa do PDV, e só reporta uma "leitura válida" quando o padrão
/// de timing entre eles for consistente com um leitor USB (ver
/// <see cref="ServicoScannerTeclado"/> para os detalhes do algoritmo).
/// </summary>
public interface IServicoScanner
{
    /// <summary>
    /// Disparado quando uma sequência de caracteres terminada em Enter é
    /// reconhecida como uma leitura de scanner válida. O argumento é o
    /// código lido (sem o Enter).
    /// </summary>
    event Action<string>? CodigoLido;

    /// <summary>
    /// Configuração de scanner atualmente em vigor (já carregada em
    /// memória — ver <see cref="RecarregarConfiguracaoAsync"/>).
    /// </summary>
    ConfiguracaoScanner ConfiguracaoAtual { get; }

    /// <summary>
    /// Regista um caractere recebido pelo campo de pesquisa do PDV. Deve
    /// ser chamado para cada tecla, na ordem em que chegam. Não faz nada
    /// se <see cref="ConfiguracaoScanner.Ativo"/> for <c>false</c>.
    /// </summary>
    void ProcessarCaracter(char caractere);

    /// <summary>
    /// Regista a tecla Enter. Decide, com base no histórico acumulado
    /// desde o último <see cref="Reset"/> (implícito ou explícito), se
    /// dispara <see cref="CodigoLido"/> (leitura reconhecida como
    /// scanner) ou se descarta o buffer em silêncio (digitação humana,
    /// ou buffer vazio) — nesse caso o fluxo de pesquisa manual já
    /// existente (Fase 7) continua a tratar o Enter da forma habitual,
    /// fora deste serviço.
    /// </summary>
    void ProcessarEnter();

    /// <summary>
    /// Limpa o buffer acumulado sem disparar <see cref="CodigoLido"/>. O
    /// Desktop deve chamar isto sempre que o campo de pesquisa for
    /// limpo ou perder o foco fora do fluxo normal de leitura (ex.: o
    /// utilizador clica noutro sítio a meio de uma leitura).
    /// </summary>
    void Reset();

    /// <summary>
    /// Recarrega <see cref="ConfiguracaoAtual"/> a partir do
    /// armazenamento local (<see cref="IArmazenamentoConfiguracaoLocal"/>).
    /// Deve ser chamado uma vez no arranque da aplicação e sempre que a
    /// Parte 4 (ecrã de Configurações do Scanner) gravar alterações —
    /// para o listener reagir de imediato, sem reiniciar a aplicação.
    /// </summary>
    Task RecarregarConfiguracaoAsync(CancellationToken cancellationToken = default);
}
