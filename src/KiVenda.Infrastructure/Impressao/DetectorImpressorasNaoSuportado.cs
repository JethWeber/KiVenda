namespace KiVenda.Infrastructure.Impressao;

public sealed class DetectorImpressorasNaoSuportado : IDetectorImpressoras
{
    public Task<IReadOnlyList<DispositivoImpressora>> DetetarAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<DispositivoImpressora>>([]);
    }
}
