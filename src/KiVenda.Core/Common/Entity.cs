namespace KiVenda.Core.Common;

/// <summary>
/// Base comum a todas as entidades do domínio. Cada entidade tem
/// identidade própria (Id), independentemente dos seus atributos.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;

    public DateTime? AtualizadoEm { get; protected set; }

    protected void MarcarComoAtualizado() => AtualizadoEm = DateTime.UtcNow;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
