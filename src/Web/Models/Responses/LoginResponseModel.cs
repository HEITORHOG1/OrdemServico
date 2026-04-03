namespace Web.Models.Responses;

public sealed record LoginResponseModel(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEm,
    UsuarioResponseModel Usuario
);
