namespace KiVenda.Infrastructure.Configuracao;

/// <summary>
/// Preferências do scanner de código de barras (Secção 6 da
/// documentação funcional). O próprio scanner funciona como um
/// periférico de teclado (não precisa de driver — ver Fase 8); esta
/// classe só guarda as opções configuráveis pelo utilizador em
/// Configurações.
/// </summary>
public sealed record ConfiguracaoScanner(
    bool Ativo = true,
    bool EmitirSomAoLer = true,
    bool AdicionarAutomaticamente = true,
    bool AbrirQuantidadeAposLeitura = false)
{
    public const string Chave = "scanner";

    public static ConfiguracaoScanner Padrao { get; } = new();
}
