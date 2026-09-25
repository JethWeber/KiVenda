using KiVenda.Application.Vendas;
using KiVenda.Infrastructure.Configuracao;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Serviço de impressão térmica ESC/POS multiplataforma.
/// A camada de infraestrutura escolhe o detector e transporte adequados ao
/// sistema operativo; a UI trabalha apenas com identificadores descobertos.
/// </summary>
public sealed class ServicoImpressaoEscPosUsb : IServicoImpressao, IServicoImpressaoTermica
{
    private readonly IArmazenamentoConfiguracaoLocal _armazenamento;
    private readonly IDetectorImpressoras _detector;
    private readonly ITransporteImpressora _transporte;
    private readonly ServicoImpressaoTexto _servicoRelatorios;

    public ServicoImpressaoEscPosUsb(
        IArmazenamentoConfiguracaoLocal armazenamento,
        IDetectorImpressoras detector,
        ITransporteImpressora transporte,
        ServicoImpressaoTexto servicoRelatorios)
    {
        _armazenamento = armazenamento;
        _detector = detector;
        _transporte = transporte;
        _servicoRelatorios = servicoRelatorios;
    }

    public async Task ImprimirReciboVendaAsync(
        ReciboVendaDto recibo,
        DadosLoja dadosLoja,
        CancellationToken cancellationToken = default)
    {
        var configuracao = await ObterConfiguracaoAsync(cancellationToken);

        if (!configuracao.Ativo)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(configuracao.Dispositivo))
        {
            throw new InvalidOperationException(
                "A impressão térmica está ativa, mas nenhuma impressora foi selecionada.");
        }

        var gerador = new GeradorEscPos(configuracao);
        var dados = gerador.GerarRecibo(recibo, dadosLoja);

        await _transporte.EnviarAsync(
            configuracao.Dispositivo,
            dados,
            cancellationToken);
    }

    public Task ImprimirTextoAsync(
        string titulo,
        string conteudo,
        CancellationToken cancellationToken = default) =>
        _servicoRelatorios.ImprimirTextoAsync(titulo, conteudo, cancellationToken);

    public async Task<IReadOnlyList<string>> ListarImpressorasDisponiveisAsync(
        CancellationToken cancellationToken = default)
    {
        var dispositivos = await _detector.DetetarAsync(cancellationToken);

        return dispositivos
            .Where(d => d.Disponivel)
            .Select(d => d.Nome)
            .ToList();
    }

    public async Task TestarImpressoraAsync(
        string dispositivo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dispositivo))
        {
            throw new InvalidOperationException(
                "Selecione uma impressora antes de executar o teste.");
        }

        var configuracao = await ObterConfiguracaoAsync(cancellationToken);
        var configuracaoTeste = configuracao with
        {
            Dispositivo = dispositivo,
            Ativo = true
        };

        var gerador = new GeradorEscPos(configuracaoTeste);
        var dados = gerador.GerarTeste("TESTE KIVENDA");

        await _transporte.EnviarAsync(
            dispositivo,
            dados,
            cancellationToken);
    }

    private async Task<ConfiguracaoImpressoraTermica> ObterConfiguracaoAsync(
        CancellationToken cancellationToken)
    {
        return await _armazenamento.ObterAsync<ConfiguracaoImpressoraTermica>(
            ConfiguracaoImpressoraTermica.Chave,
            cancellationToken)
            ?? ConfiguracaoImpressoraTermica.Padrao;
    }
}
