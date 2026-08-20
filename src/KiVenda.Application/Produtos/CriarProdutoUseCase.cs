using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

public sealed record CriarProdutoCommand(
    string Nome,
    string CodigoInterno,
    Guid CategoriaId,
    Guid UnidadeBaseId,
    decimal PrecoVendaPorUnidadeBase,
    decimal StockMinimo,
    string? CodigoBarras = null,
    string? FotoUrl = null);

public sealed class CriarProdutoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(CriarProdutoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CadastrarProdutos);

        var existente = await uow.Produtos.ObterPorCodigoInternoAsync(comando.CodigoInterno, cancellationToken);
        if (existente is not null)
        {
            throw new DomainException($"Já existe um produto com o código \"{comando.CodigoInterno}\".");
        }

        _ = await uow.Categorias.ObterPorIdAsync(comando.CategoriaId, cancellationToken)
            ?? throw new DomainException("Categoria não encontrada.");

        _ = await uow.UnidadesMedida.ObterPorIdAsync(comando.UnidadeBaseId, cancellationToken)
            ?? throw new DomainException("Unidade de medida base não encontrada.");

        var produto = new Produto(
            comando.Nome,
            comando.CodigoInterno,
            comando.CategoriaId,
            comando.UnidadeBaseId,
            comando.PrecoVendaPorUnidadeBase,
            comando.StockMinimo,
            comando.CodigoBarras,
            comando.FotoUrl);

        await uow.Produtos.AdicionarAsync(produto, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return produto.Id;
    }
}
