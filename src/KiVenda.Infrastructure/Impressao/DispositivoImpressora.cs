namespace KiVenda.Infrastructure.Impressao;

public enum EstadoDispositivoImpressora
{
    Disponivel,
    SemPermissao,
    Indisponivel,
    Erro
}

public sealed record DispositivoImpressora(
    string Id,
    string Nome,
    string SistemaOperativo,
    string Porta,
    EstadoDispositivoImpressora Estado,
    string? Detalhe = null)
{
    public bool Disponivel => Estado == EstadoDispositivoImpressora.Disponivel;
}
