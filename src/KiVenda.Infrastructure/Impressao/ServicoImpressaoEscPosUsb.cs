using KiVenda.Application.Vendas;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Serviço de impressão térmica baseado em ESC/POS.
/// A geração dos comandos está separada do transporte USB para permitir
/// testes sem impressora física e futuras adaptações de transporte.
/// </summary>
public sealed class ServicoImpressaoEscPosUsb : IServicoImpressao
{
    private readonly ConfiguracaoImpressoraTermica _configuracao;
    private readonly GeradorEscPos _gerador;
    private readonly TransporteImpressoraUsb _transporte;
    private readonly ServicoImpressaoTexto _servicoRelatorios;

    public ServicoImpressaoEscPosUsb(
        ConfiguracaoImpressoraTermica configuracao,
        TransporteImpressoraUsb transporte,
        ServicoImpressaoTexto servicoRelatorios)
    {
        _configuracao = configuracao;
        _gerador = new GeradorEscPos(configuracao);
        _transporte = transporte;
        _servicoRelatorios = servicoRelatorios;
    }

    public async Task ImprimirReciboVendaAsync(
        ReciboVendaDto recibo,
        DadosLoja dadosLoja,
        CancellationToken cancellationToken = default)
    {
        var dados = _gerador.GerarRecibo(recibo, dadosLoja);
        await _transporte.EnviarAsync(_configuracao.Dispositivo, dados, cancellationToken);
    }

    /// <summary>
    /// Relatórios continuam a usar a impressão normal/textual.
    /// A camada térmica fica exclusivamente responsável por recibos.
    /// </summary>
    public Task ImprimirTextoAsync(
        string titulo,
        string conteudo,
        CancellationToken cancellationToken = default) =>
        _servicoRelatorios.ImprimirTextoAsync(titulo, conteudo, cancellationToken);

    public Task<IReadOnlyList<string>> ListarImpressorasDisponiveisAsync(
        CancellationToken cancellationToken = default)
    {
        if (File.Exists(_configuracao.Dispositivo))
        {
            return Task.FromResult<IReadOnlyList<string>>(
                [$"Térmica ESC/POS ({_configuracao.Dispositivo})"]);
        }

        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }
}
