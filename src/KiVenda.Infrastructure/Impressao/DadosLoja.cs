namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Dados da loja usados no cabeçalho do recibo (Secção 4.9,
/// "Configurações → Dados da Loja"). Modelo mínimo para a Fase 4; o
/// ecrã completo de edição (com logótipo) pertence à Fase 11 — esta
/// classe só define o que o serviço de impressão precisa de conhecer.
/// </summary>
public sealed record DadosLoja(string Nome, string? Endereco = null, string? Contacto = null);
