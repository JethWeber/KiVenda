using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;

namespace KiVenda.Application.Utilizadores;

public sealed record EditarMeusDadosCommand(Guid UtilizadorId, string Nome, string NomeUtilizador, string? NovaSenha);

public sealed class EditarMeusDadosUseCase(
    IUnitOfWork uow,
    IContextoAutenticacao contexto,
    ISenhaHasher senhaHasher)
{
    public async Task ExecutarAsync(EditarMeusDadosCommand comando, CancellationToken cancellationToken = default)
    {
        if (comando.UtilizadorId != contexto.UtilizadorId)
            throw new DomainException("Só podes alterar os teus próprios dados.");

        if (string.IsNullOrWhiteSpace(comando.Nome) || string.IsNullOrWhiteSpace(comando.NomeUtilizador))
            throw new DomainException("Nome e login são obrigatórios.");

        var utilizador = await uow.Utilizadores.ObterPorIdAsync(comando.UtilizadorId, cancellationToken)
            ?? throw new DomainException("Utilizador não encontrado.");

        var existente = await uow.Utilizadores.ObterPorNomeUtilizadorAsync(comando.NomeUtilizador.Trim(), cancellationToken);
        if (existente is not null && existente.Id != utilizador.Id)
            throw new DomainException("Esse login já está em uso.");

        utilizador.AlterarDados(comando.Nome, comando.NomeUtilizador);
        if (!string.IsNullOrWhiteSpace(comando.NovaSenha))
            utilizador.AlterarPasswordHash(senhaHasher.GerarHash(comando.NovaSenha));

        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(contexto.UtilizadorId, "Alterou os próprios dados", "Utilizador", utilizador.Id),
            cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }
}