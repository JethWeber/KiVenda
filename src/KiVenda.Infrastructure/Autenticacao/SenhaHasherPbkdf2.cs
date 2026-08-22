using System.Security.Cryptography;
using KiVenda.Application.Abstractions.Auth;

namespace KiVenda.Infrastructure.Autenticacao;

/// <summary>
/// Implementa <see cref="ISenhaHasher"/> (contrato definido na
/// Application, Fase 3) usando PBKDF2/SHA-256 — disponível diretamente
/// em <c>System.Security.Cryptography</c>, sem exigir nenhum pacote
/// NuGet de terceiros (ex.: BCrypt.Net). Formato do hash guardado:
/// <c>{iterações}.{salt em Base64}.{hash em Base64}</c>, para que o
/// número de iterações possa aumentar no futuro sem invalidar hashes
/// já guardados (cada hash sabe com quantas iterações foi gerado).
/// </summary>
public sealed class SenhaHasherPbkdf2 : ISenhaHasher
{
    private const int TamanhoSaltBytes = 16;
    private const int TamanhoHashBytes = 32;
    private const int Iteracoes = 100_000;
    private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA256;

    public string GerarHash(string senhaEmClaro)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senhaEmClaro, salt, Iteracoes, Algoritmo, TamanhoHashBytes);

        return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string senhaEmClaro, string hashArmazenado)
    {
        var partes = hashArmazenado.Split('.');
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteracoes))
        {
            return false;
        }

        byte[] salt;
        byte[] hashEsperado;
        try
        {
            salt = Convert.FromBase64String(partes[1]);
            hashEsperado = Convert.FromBase64String(partes[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senhaEmClaro, salt, iteracoes, Algoritmo, hashEsperado.Length);

        // Comparação em tempo constante — evita que o tempo de resposta
        // revele quantos bytes do hash coincidem (timing attack).
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
