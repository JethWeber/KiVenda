using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

public sealed record CriarApresentacaoProdutoCommand(
    Guid ProdutoId,
    string Nome,
    decimal FatorConversaoParaUnidadeBase,
    string? CodigoBarras = null);

public sealed class CriarApresentacaoProdutoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(CriarApresentacaoProdutoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CadastrarProdutos);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var apresentacao = produto.AdicionarApresentacao(comando.Nome, comando.FatorConversaoParaUnidadeBase, comando.CodigoBarras);

        await uow.SaveChangesAsync(cancellationToken);

        return apresentacao.Id;
    }
}
