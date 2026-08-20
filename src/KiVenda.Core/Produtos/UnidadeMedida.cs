using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Produtos;

/// <summary>
/// Unidade indivisível em que o estoque de um produto é sempre guardado
/// e movimentado (ex.: "un", "g", "ml"). As <see cref="ApresentacaoProduto"/>
/// de um produto convertem-se sempre para/de esta unidade.
/// </summary>
public sealed class UnidadeMedida : Entity
{
    public string Codigo { get; private set; } = null!;

    public string Nome { get; private set; } = null!;

    private UnidadeMedida()
    {
        // Construtor privado para uso do EF Core (Fase 2).
    }

    public UnidadeMedida(string codigo, string nome)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new DomainException("O código da unidade de medida é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome da unidade de medida é obrigatório.");
        }

        Codigo = codigo.Trim();
        Nome = nome.Trim();
    }

    /// <summary>
    /// Unidades de medida padrão, disponíveis desde a primeira execução
    /// (seed inicial — ver Fase 2). Mantidas aqui como referência única
    /// dos códigos "conhecidos" pelo sistema.
    /// </summary>
    public static class Padrao
    {
        public const string Unidade = "un";
        public const string Grama = "g";
        public const string Mililitro = "ml";
    }
}
