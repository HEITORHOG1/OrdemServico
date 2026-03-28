namespace Application.DTOs.Auth;

public sealed record AlterarSenhaRequest(
    string SenhaAtual,
    string NovaSenha,
    string ConfirmarNovaSenha
);
