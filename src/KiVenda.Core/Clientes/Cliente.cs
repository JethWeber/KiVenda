using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Clientes;

/// <summary>
/// Cadastro simplificado de cliente, sem controlo de dívida/fiado no
/// MVP (fora de escopo — ver plano de implementação). Toda venda é
/// considerada venda paga.
/// </summary>
public sealed class Cliente : Entity
{
    public string Nome { get; private set; } = null!;

    public string? Telefone { get; private set; }

    private Cliente()
    {
    }

    public Cliente(string nome, string? telefone = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do cliente é obrigatório.");
        }

        Nome = nome.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
    }

    public void EditarDados(string nome, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do cliente é obrigatório.");
        }

        Nome = nome.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        MarcarComoAtualizado();
    }
}
