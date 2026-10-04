using System.Globalization;
using System.Text;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using KiVenda.Application.Vendas;

namespace KiVenda.Infrastructure.Impressao;

/// <summary>
/// Monta o recibo em comandos ESC/POS através do ESCPOS.NET.
/// Não conhece o transporte nem o sistema operativo.
/// </summary>
public sealed class GeradorEscPos
{
    private readonly ConfiguracaoImpressoraTermica _configuracao;

    public GeradorEscPos(ConfiguracaoImpressoraTermica configuracao)
    {
        _configuracao = configuracao;
        _configuracao.Validar();
    }

    public byte[] GerarRecibo(ReciboVendaDto recibo, DadosLoja dadosLoja)
    {
        var e = CriarEmitter();
        var cultura = ObterCultura();
        var linhas = new List<byte[]>
        {
            e.Initialize(),
            e.CenterAlign(),
            e.SetStyles(PrintStyle.Bold | PrintStyle.DoubleHeight),
            e.PrintLine(Limitar(dadosLoja.Nome)),
            e.SetStyles(PrintStyle.None)
        };

        if (!string.IsNullOrWhiteSpace(dadosLoja.Nif))
            linhas.Add(e.PrintLine(Limitar($"NIF: {dadosLoja.Nif}")));

        linhas.Add(e.SetStyles(PrintStyle.None));
        linhas.Add(e.LeftAlign());
        linhas.Add(e.PrintLine(Separador('-')));
        linhas.Add(e.SetStyles(PrintStyle.Bold));
        linhas.Add(e.PrintLine($"RECIBO DE VENDA {recibo.VendaId.ToString()[..8].ToUpperInvariant()}"));
        linhas.Add(e.SetStyles(PrintStyle.None));
        linhas.Add(e.PrintLine($"Data: {recibo.Data.ToLocalTime():dd/MM/yyyy HH:mm}"));
        linhas.Add(e.PrintLine($"Operador: {Limitar(recibo.OperadorNome)}"));
        linhas.Add(e.PrintLine("Cliente: Consumidor Final"));
        linhas.Add(e.PrintLine(Separador('-')));

        foreach (var item in recibo.Itens)
        {
            foreach (var linha in QuebrarTexto(item.ProdutoNome))
                linhas.Add(e.PrintLine(linha));

            var quantidade = item.QuantidadeNaApresentacao.ToString("0.##", cultura);
            var total = $"{item.ValorTotal.ToString("N2", cultura)} Kz";
            linhas.Add(e.PrintLine(ColunasDuplas(
                $"{quantidade} {item.ApresentacaoNome}",
                total)));
        }

        linhas.Add(e.PrintLine(Separador('-')));
        linhas.Add(e.PrintLine(ColunasDuplas("Subtotal:", $"{recibo.Subtotal.ToString("N2", cultura)} Kz")));
        linhas.Add(e.SetStyles(PrintStyle.Bold));
        linhas.Add(e.PrintLine(ColunasDuplas("TOTAL A PAGAR:", $"{recibo.Total.ToString("N2", cultura)} Kz")));
        linhas.Add(e.SetStyles(PrintStyle.None));
        linhas.Add(e.PrintLine($"Pagamento: {recibo.MetodoPagamento}"));
        linhas.Add(e.PrintLine(Separador('=')));

        linhas.Add(e.CenterAlign());
        foreach (var texto in new[] { dadosLoja.Endereco, 
                                      string.Join(" - ", new[] { dadosLoja.Municipio, dadosLoja.Provincia }.Where(x => !string.IsNullOrWhiteSpace(x))),
                                      string.IsNullOrWhiteSpace(dadosLoja.Contacto) ? null : $"Tel: {dadosLoja.Contacto}",
                                      dadosLoja.Website,
                                      "Obrigado pela preferência!",
                                      "Volte Sempre!",
                                      "PROCESSADO POR COMPUTADOR",
                                      "KiVenda Desktop" })
        {
            if (!string.IsNullOrWhiteSpace(texto))
                foreach (var linha in QuebrarTexto(texto))
                    linhas.Add(e.PrintLine(linha));
        }

        linhas.Add(e.FeedLines(Math.Clamp(_configuracao.LinhasAlimentacaoFinal, 0, 20)));

        if (_configuracao.CortarPapel)
            linhas.Add(e.FullCut());

        return ByteSplicer.Combine(linhas.ToArray());
    }

    public byte[] GerarTeste(string texto)
    {
        var e = CriarEmitter();
        var linhas = new List<byte[]>
        {
            e.Initialize(),
            e.CenterAlign(),
            e.SetStyles(PrintStyle.Bold | PrintStyle.DoubleHeight),
            e.PrintLine("KiVenda"),
            e.SetStyles(PrintStyle.None),
            e.PrintLine("Teste de impressão ESC/POS"),
            e.PrintLine(texto),
            e.PrintLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")),
            e.FeedLines(3)
        };

        if (_configuracao.CortarPapel)
            linhas.Add(e.FullCut());

        return ByteSplicer.Combine(linhas.ToArray());
    }

    private EPSON CriarEmitter() => new() { Encoding = _configuracao.ObterEncoding() };

    private string Separador(char caractere) =>
        new(caractere, Math.Max(1, _configuracao.Colunas));

    private string ColunasDuplas(string esquerda, string direita)
    {
        var largura = Math.Max(24, _configuracao.Colunas);
        var maxEsquerda = Math.Max(1, largura - direita.Length - 1);
        if (esquerda.Length > maxEsquerda)
            esquerda = esquerda[..maxEsquerda];

        return esquerda.PadRight(largura - direita.Length) + direita;
    }

    private string Limitar(string texto) =>
        string.IsNullOrWhiteSpace(texto)
            ? string.Empty
            : QuebrarTexto(texto).FirstOrDefault() ?? string.Empty;

    private IEnumerable<string> QuebrarTexto(string texto)
    {
        var largura = Math.Max(1, _configuracao.Colunas);
        if (string.IsNullOrWhiteSpace(texto))
            yield break;

        foreach (var paragrafo in texto.Replace("\r", string.Empty).Split('\n'))
        {
            var palavras = paragrafo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var linha = new StringBuilder();

            foreach (var palavra in palavras)
            {
                if (palavra.Length > largura)
                {
                    if (linha.Length > 0)
                    {
                        yield return linha.ToString();
                        linha.Clear();
                    }

                    for (var i = 0; i < palavra.Length; i += largura)
                        yield return palavra.Substring(i, Math.Min(largura, palavra.Length - i));

                    continue;
                }

                if (linha.Length > 0 && linha.Length + palavra.Length + 1 > largura)
                {
                    yield return linha.ToString();
                    linha.Clear();
                }

                if (linha.Length > 0)
                    linha.Append(' ');

                linha.Append(palavra);
            }

            if (linha.Length > 0)
                yield return linha.ToString();
        }
    }

    private static CultureInfo ObterCultura()
    {
        var cultura = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ",";
        cultura.NumberFormat.NumberGroupSeparator = ".";
        return cultura;
    }
}
