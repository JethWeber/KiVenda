using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Vendas;

public sealed record CancelarVendaCommand(Guid VendaId);

/// <summary>
/// Cancela uma venda em andamento (Secção 4.4: "Cancelamento de venda
/// em curso"). Não faz nada ao stock/caixa — só chega lá se a venda
/// nunca tiver sido finalizada (o Core impede cancelar uma venda já
/// finalizada, ver <see cref="Core.Vendas.Venda.Cancelar"/>), logo
/// nunca houve saída de stock nem entrada de caixa a desfazer.
/// </summary>
public sealed class CancelarVendaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task ExecutarAsync(CancelarVendaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.FazerVenda);

        var venda = await uow.Vendas.ObterPorIdAsync(comando.VendaId, cancellationToken)
            ?? throw new DomainException("Venda não encontrada.");

        venda.Cancelar();

        await uow.SaveChangesAsync(cancellationToken);
    }
}
