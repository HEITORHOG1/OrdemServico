namespace Application.DTOs.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEm,
    UsuarioResponse Usuario
);
