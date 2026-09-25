using KiVenda.Application.Vendas;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Emite o recibo de uma venda finalizada. Consumido pelo Desktop
/// depois de <c>FinalizarVendaUseCase</c> devolver um
/// <see cref="ReciboVendaDto"/> — não é chamado pela Application, para
/// manter essa camada livre de detalhes de hardware/impressora.
/// </summary>
public interface IServicoImpressao
{
    Task ImprimirReciboVendaAsync(ReciboVendaDto recibo, DadosLoja dadosLoja, CancellationToken cancellationToken = default);

    /// <summary>Impressão de texto livre, reaproveitada pelos Relatórios (Fase 9).</summary>
    Task ImprimirTextoAsync(string titulo, string conteudo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListarImpressorasDisponiveisAsync(CancellationToken cancellationToken = default);
}
