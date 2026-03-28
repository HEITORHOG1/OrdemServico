using Domain.Enums;

namespace Application.DTOs.Auth;

public sealed record RegistrarUsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    string ConfirmarSenha,
    CargoUsuario Cargo
);
