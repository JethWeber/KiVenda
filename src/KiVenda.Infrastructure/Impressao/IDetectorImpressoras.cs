namespace KiVenda.Infrastructure.Impressao;

public interface IDetectorImpressoras
{
    Task<IReadOnlyList<DispositivoImpressora>> DetetarAsync(
        CancellationToken cancellationToken = default);
}
