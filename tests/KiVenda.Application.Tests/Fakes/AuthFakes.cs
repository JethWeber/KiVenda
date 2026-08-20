using KiVenda.Application.Abstractions.Auth;
using KiVenda.Core.Enums;

namespace KiVenda.Application.Tests.Fakes;

public sealed class FakeContextoAutenticacao : IContextoAutenticacao
{
    public Guid UtilizadorId { get; set; } = Guid.NewGuid();

    public PerfilUtilizador Perfil { get; set; } = PerfilUtilizador.Gerente;
}

/// <summary>Hash "falso" (prefixo fixo) — suficiente para testar o fluxo, nunca usar em produção.</summary>
public sealed class FakeSenhaHasher : ISenhaHasher
{
    public string GerarHash(string senhaEmClaro) => $"hash:{senhaEmClaro}";

    public bool Verificar(string senhaEmClaro, string hashArmazenado) => hashArmazenado == $"hash:{senhaEmClaro}";
}
