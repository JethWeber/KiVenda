using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;

namespace KiVenda.Application.Vendas;

public sealed record IniciarVendaCommand(Guid? ClienteId = null);

public sealed class IniciarVendaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(IniciarVendaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.FazerVenda);

        var sessaoAberta = await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken)
            ?? throw new DomainException("Não há nenhuma sessão de caixa aberta. Abra o caixa antes de iniciar uma venda.");

        if (comando.ClienteId.HasValue)
        {
            _ = await uow.Clientes.ObterPorIdAsync(comando.ClienteId.Value, cancellationToken)
                ?? throw new DomainException("Cliente não encontrado.");
        }

        var venda = new Venda(contexto.UtilizadorId, sessaoAberta.Id, comando.ClienteId);

        await uow.Vendas.AdicionarAsync(venda, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return venda.Id;
    }
}
