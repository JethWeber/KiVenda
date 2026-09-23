using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

        // Diagnóstico temporário: confirmar como o EF está classificando
        // o ItemVenda recém-adicionado ao agregado.
        var context = uow.GetType()
            .GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(uow) as DbContext;

        if (context is not null)
        {
            var entityType = context.Model.FindEntityType(typeof(ItemVenda));
            var key = entityType?.FindPrimaryKey();

            Console.WriteLine("===== ITEM VENDA DEBUG =====");
            Console.WriteLine($"Id: {item.Id}");
            Console.WriteLine($"Guid.Empty: {item.Id == Guid.Empty}");
            Console.WriteLine($"ValueGenerated: {key?.Properties.Single().ValueGenerated}");

            foreach (var entry in context.ChangeTracker.Entries<ItemVenda>())
            {
                Console.WriteLine(
                    $"TRACKER -> State={entry.State}, Id={entry.Entity.Id}");
            }

            Console.WriteLine("============================");
        }

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
