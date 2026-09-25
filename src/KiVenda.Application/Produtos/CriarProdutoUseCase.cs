using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

public sealed record CriarProdutoCommand(
    string Nome,
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

        _ = await uow.Categorias.ObterPorIdAsync(comando.CategoriaId, cancellationToken)
            ?? throw new DomainException("Categoria não encontrada.");

        _ = await uow.UnidadesMedida.ObterPorIdAsync(comando.UnidadeBaseId, cancellationToken)
            ?? throw new DomainException("Unidade de medida base não encontrada.");

        var codigoInterno = await uow.Produtos.ObterProximoCodigoInternoAsync(cancellationToken);

        var produto = new Produto(
            comando.Nome,
            codigoInterno,
            comando.CategoriaId,
            comando.UnidadeBaseId,
            comando.PrecoVendaPorUnidadeBase,
            comando.StockMinimo,
            comando.CodigoBarras,
            comando.FotoUrl);

        await uow.Produtos.AdicionarAsync(produto, cancellationToken);

        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(contexto.UtilizadorId, "Criou Produto", "Produto", produto.Id, dadosDepois: $"Código: {produto.CodigoInterno}; Nome: {produto.Nome}"),
            cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return produto.Id;
    }
}
