using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Produtos;

public sealed class Categoria : Entity
{
    public string Nome { get; private set; } = null!;

    private Categoria()
    {
    }

    public Categoria(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome da categoria é obrigatório.");
        }

        Nome = nome.Trim();
    }

    public void Renomear(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
        {
            throw new DomainException("O nome da categoria é obrigatório.");
        }

        Nome = novoNome.Trim();
        MarcarComoAtualizado();
    }
}
