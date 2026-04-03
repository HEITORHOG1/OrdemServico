namespace Web.Models.Requests;

public sealed record LoginRequestModel(
    string Email,
    string Senha
);
