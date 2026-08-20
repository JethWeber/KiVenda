namespace KiVenda.Core.Enums;

/// <summary>
/// Perfis de acesso previstos no MVP (ver Secção 5 da documentação
/// funcional). "Caixa" e "Atendente N" da estrutura organizacional
/// mapeiam-se, em termos de permissões, para <see cref="Atendente"/>.
/// </summary>
public enum PerfilUtilizador
{
    Gerente = 1,
    Atendente = 2
}
