using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

public sealed record EditarProdutoCommand(
    Guid ProdutoId,
    string Nome,
    decimal PrecoVendaPorUnidadeBase,
    decimal StockMinimo,
    string? CodigoBarras,
    string? FotoUrl);

public sealed class EditarProdutoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(EditarProdutoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.CadastrarProdutos);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var precoAnterior = produto.PrecoVendaPorUnidadeBase;

        produto.EditarDadosBasicos(comando.Nome, comando.PrecoVendaPorUnidadeBase, comando.StockMinimo, comando.CodigoBarras, comando.FotoUrl);

        if (precoAnterior != comando.PrecoVendaPorUnidadeBase)
        {
            // Alteração de preço é uma das operações sensíveis explicitamente
            // referidas na Secção 7 (Auditoria) da documentação funcional.
            await uow.LogsAuditoria.AdicionarAsync(
                new LogAuditoria(
                    contexto.UtilizadorId,
                    "Alterou preço",
                    "Produto",
                    produto.Id,
                    dadosAntes: precoAnterior.ToString("0.####"),
                    dadosDepois: comando.PrecoVendaPorUnidadeBase.ToString("0.####")),
                cancellationToken);
        }

        await uow.SaveChangesAsync(cancellationToken);
    }
}
