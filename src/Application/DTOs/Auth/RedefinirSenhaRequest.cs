namespace Application.DTOs.Auth;

public sealed record RedefinirSenhaRequest(
    string Email,
    string Token,
    string NovaSenha,
    string ConfirmarNovaSenha
);
