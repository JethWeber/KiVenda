namespace KiVenda.Infrastructure.Impressao;

public sealed record OpcaoConexaoImpressora(
    TipoConexaoImpressora Valor,
    string Nome,
    string Descricao);
