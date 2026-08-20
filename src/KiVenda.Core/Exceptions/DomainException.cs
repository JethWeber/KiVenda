namespace KiVenda.Core.Exceptions;

/// <summary>
/// Lançada quando uma regra de negócio do domínio é violada
/// (ex.: stock insuficiente, valores negativos, fator de conversão inválido).
/// Não deve ser usada para erros técnicos/infraestrutura — esses pertencem
/// às camadas superiores.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
