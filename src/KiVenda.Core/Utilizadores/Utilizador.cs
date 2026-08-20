using KiVenda.Core.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Utilizadores;

/// <summary>
/// Toda venda e movimento de caixa fica associado ao utilizador que a
/// realizou (Secção 5 da documentação funcional). O hashing efetivo da
/// password é responsabilidade da Infrastructure (Fase 4); o Core só
/// guarda e valida a presença do hash já calculado.
/// </summary>
public sealed class Utilizador : Entity
{
    public string Nome { get; private set; } = null!;

    public string NomeUtilizador { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public PerfilUtilizador Perfil { get; private set; }

    public bool Ativo { get; private set; } = true;

    private Utilizador()
    {
    }

    public Utilizador(string nome, string nomeUtilizador, string passwordHash, PerfilUtilizador perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do utilizador é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(nomeUtilizador))
        {
            throw new DomainException("O nome de utilizador (login) é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("A password do utilizador é obrigatória.");
        }

        Nome = nome.Trim();
        NomeUtilizador = nomeUtilizador.Trim();
        PasswordHash = passwordHash;
        Perfil = perfil;
    }

    public void AlterarPasswordHash(string novoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(novoPasswordHash))
        {
            throw new DomainException("A password do utilizador é obrigatória.");
        }

        PasswordHash = novoPasswordHash;
        MarcarComoAtualizado();
    }

    public void DefinirPerfil(PerfilUtilizador perfil)
    {
        Perfil = perfil;
        MarcarComoAtualizado();
    }

    public void Inativar()
    {
        Ativo = false;
        MarcarComoAtualizado();
    }

    public void Reativar()
    {
        Ativo = true;
        MarcarComoAtualizado();
    }

    public bool PodeExecutar(Acao acao) => Ativo && Permissoes.Permite(Perfil, acao);
}
