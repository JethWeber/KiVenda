using System.Text;
using KiVenda.Application.Vendas;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Gera bytes ESC/POS sem conhecer USB, serial, rede ou qualquer outro
/// transporte. Isto permite testar a impressão sem hardware real.
/// </summary>
public sealed class GeradorEscPos
{
    private static readonly byte[] Inicializar = [0x1B, 0x40];
    private readonly ConfiguracaoImpressoraTermica _configuracao;

    public GeradorEscPos(ConfiguracaoImpressoraTermica configuracao)
    {
        _configuracao = configuracao;
    }

    public byte[] GerarRecibo(ReciboVendaDto recibo, DadosLoja dadosLoja)
    {
        var encoding = _configuracao.ObterEncoding();
        using var stream = new MemoryStream();

        Escrever(stream, Inicializar);
        DefinirAlinhamento(stream, 1);
        DefinirNegrito(stream, true);
        EscreverTexto(stream, dadosLoja.Nome, encoding);
        DefinirNegrito(stream, false);

        if (!string.IsNullOrWhiteSpace(dadosLoja.Nif))
            EscreverTexto(stream, $"NIF: {dadosLoja.Nif}", encoding);

        DefinirAlinhamento(stream, 0);
        EscreverLinha(stream, new string('-', _configuracao.Colunas), encoding);
        EscreverLinha(stream, $"FATURA {recibo.VendaId.ToString()[..8].ToUpperInvariant()}", encoding);
        EscreverLinha(stream, $"Data: {recibo.Data.ToLocalTime():dd/MM/yyyy HH:mm}", encoding);
        EscreverLinha(stream, $"Operador: {recibo.OperadorNome}", encoding);
        EscreverLinha(stream, "Cliente: Consumidor Final", encoding);
        EscreverLinha(stream, new string('-', _configuracao.Colunas), encoding);

        foreach (var item in recibo.Itens)
        {
            EscreverLinha(stream, item.ProdutoNome, encoding);
            var quantidade = item.QuantidadeNaApresentacao.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            var total = item.ValorTotal.ToString("N2", ObterCultura());
            EscreverLinha(stream, $"  {quantidade} {item.ApresentacaoNome}  {total} Kz", encoding);
        }

        EscreverLinha(stream, new string('-', _configuracao.Colunas), encoding);
        EscreverLinha(stream, $"Subtotal: {recibo.Subtotal.ToString("N2", ObterCultura())} Kz", encoding);

        DefinirNegrito(stream, true);
        EscreverLinha(stream, $"TOTAL A PAGAR: {recibo.Total.ToString("N2", ObterCultura())} Kz", encoding);
        DefinirNegrito(stream, false);

        EscreverLinha(stream, $"Pagamento: {recibo.MetodoPagamento}", encoding);
        EscreverLinha(stream, new string('=', _configuracao.Colunas), encoding);

        DefinirAlinhamento(stream, 1);
        if (!string.IsNullOrWhiteSpace(dadosLoja.Endereco))
            EscreverTexto(stream, dadosLoja.Endereco, encoding);

        var localizacao = string.Join(" - ", new[] { dadosLoja.Municipio, dadosLoja.Provincia }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(localizacao))
            EscreverTexto(stream, localizacao, encoding);

        if (!string.IsNullOrWhiteSpace(dadosLoja.Contacto))
            EscreverTexto(stream, $"Tel: {dadosLoja.Contacto}", encoding);

        if (!string.IsNullOrWhiteSpace(dadosLoja.Website))
            EscreverTexto(stream, dadosLoja.Website, encoding);

        EscreverTexto(stream, "Obrigado pela preferência!", encoding);
        EscreverTexto(stream, "Volte Sempre!", encoding);
        EscreverTexto(stream, string.Empty, encoding);
        EscreverTexto(stream, "PROCESSADO POR COMPUTADOR", encoding);
        EscreverTexto(stream, "KiVenda Desktop", encoding);

        Escrever(stream, [0x1B, 0x64, (byte)Math.Clamp(_configuracao.LinhasAlimentacaoFinal, 0, 255)]);

        if (_configuracao.CortarPapel)
            Escrever(stream, [0x1D, 0x56, 0x00]);

        return stream.ToArray();
    }

    public byte[] GerarTeste(string texto)
    {
        var encoding = _configuracao.ObterEncoding();
        using var stream = new MemoryStream();

        Escrever(stream, Inicializar);
        DefinirAlinhamento(stream, 1);
        DefinirNegrito(stream, true);
        EscreverTexto(stream, "KiVenda ESC/POS", encoding);
        DefinirNegrito(stream, false);
        EscreverTexto(stream, texto, encoding);
        Escrever(stream, [0x1B, 0x64, 0x03]);

        if (_configuracao.CortarPapel)
            Escrever(stream, [0x1D, 0x56, 0x00]);

        return stream.ToArray();
    }

    private static void DefinirAlinhamento(Stream stream, byte alinhamento) =>
        Escrever(stream, [0x1B, 0x61, alinhamento]);

    private static void DefinirNegrito(Stream stream, bool ativo) =>
        Escrever(stream, [0x1B, 0x45, ativo ? (byte)1 : (byte)0]);

    private static void EscreverTexto(Stream stream, string texto, Encoding encoding) =>
        EscreverLinha(stream, texto, encoding);

    private static void EscreverLinha(Stream stream, string texto, Encoding encoding)
    {
        Escrever(stream, encoding.GetBytes(texto));
        Escrever(stream, [0x0A]);
    }

    private static void Escrever(Stream stream, byte[] bytes) =>
        stream.Write(bytes, 0, bytes.Length);

    private static System.Globalization.CultureInfo ObterCultura()
    {
        var cultura = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.InvariantCulture.Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ",";
        cultura.NumberFormat.NumberGroupSeparator = ".";
        return cultura;
    }
}
