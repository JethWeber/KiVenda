namespace KiVenda.Infrastructure.Licenciamento;

/// <summary>
/// Ativação e validação da licença local (Secção 4.9,
/// "Configurações → Licença"; e Secção 9, "Licenciamento: integração
/// com o sistema de licenciamento corporativo da Weber Tech — par de
/// chaves pública/privada e ficheiro de licença .wta, documentado em
/// separado"). Chamado pelo Desktop na inicialização (Fase 6) para
/// decidir se a aplicação pode arrancar.
/// </summary>
public interface IServicoLicenciamento
{
    Task<ResultadoValidacaoLicenca> ValidarLicencaAtualAsync(CancellationToken cancellationToken = default);

    Task<ResultadoValidacaoLicenca> AtivarLicencaAsync(string caminhoFicheiroLicenca, CancellationToken cancellationToken = default);
}
