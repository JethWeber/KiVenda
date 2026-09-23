using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Vendas;

public sealed record AdicionarItemVendaCommand(Guid VendaId, Guid ProdutoId, Guid ApresentacaoId, decimal QuantidadeNaApresentacao);

public sealed record RemoverItemVendaCommand(Guid VendaId, Guid ItemId);

public sealed record AplicarDescontoVendaCommand(Guid VendaId, decimal ValorDesconto);

public sealed class AdicionarItemVendaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(AdicionarItemVendaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.FazerVenda);

        var venda = await uow.Vendas.ObterPorIdAsync(comando.VendaId, cancellationToken)
            ?? throw new DomainException("Venda não encontrada.");

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var item = venda.AdicionarItem(produto, comando.ApresentacaoId, comando.QuantidadeNaApresentacao);

        await uow.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}

public sealed class RemoverItemVendaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(RemoverItemVendaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.FazerVenda);

        var venda = await uow.Vendas.ObterPorIdAsync(comando.VendaId, cancellationToken)
            ?? throw new DomainException("Venda não encontrada.");

        venda.RemoverItem(comando.ItemId);

        await uow.SaveChangesAsync(cancellationToken);
    }
}

public sealed class AplicarDescontoVendaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(AplicarDescontoVendaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.FazerVenda);

        var venda = await uow.Vendas.ObterPorIdAsync(comando.VendaId, cancellationToken)
            ?? throw new DomainException("Venda não encontrada.");

        venda.AplicarDesconto(comando.ValorDesconto);

        await uow.SaveChangesAsync(cancellationToken);
    }
}
