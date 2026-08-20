namespace KiVenda.Application.Abstractions.Auth;

/// <summary>
/// Cálculo e verificação de hash de password. O Core nunca lida com
/// texto em claro nem com o algoritmo de hashing (ver Fase 1: Utilizador
/// só guarda <c>PasswordHash</c>) — este contrato é implementado pela
/// Infrastructure (Fase 4), que escolhe o algoritmo (ex.: BCrypt/Argon2).
/// </summary>
public interface ISenhaHasher
{
    string GerarHash(string senhaEmClaro);

    bool Verificar(string senhaEmClaro, string hashArmazenado);
}
