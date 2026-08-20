using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Fornecedores;

public sealed class Fornecedor : Entity
{
    public string Nome { get; private set; } = null!;

    public string? Telefone { get; private set; }

    /// <summary>Descrição livre dos produtos fornecidos, para agilizar o registo de compras futuras.</summary>
    public string? ProdutosFornecidos { get; private set; }

    private Fornecedor()
    {
    }

    public Fornecedor(string nome, string? telefone = null, string? produtosFornecidos = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do fornecedor é obrigatório.");
        }

        Nome = nome.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        ProdutosFornecidos = string.IsNullOrWhiteSpace(produtosFornecidos) ? null : produtosFornecidos.Trim();
    }

    public void EditarDados(string nome, string? telefone, string? produtosFornecidos)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do fornecedor é obrigatório.");
        }

        Nome = nome.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        ProdutosFornecidos = string.IsNullOrWhiteSpace(produtosFornecidos) ? null : produtosFornecidos.Trim();
        MarcarComoAtualizado();
    }
}
