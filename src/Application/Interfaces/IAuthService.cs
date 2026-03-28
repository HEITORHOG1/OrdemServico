using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<UsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request, Guid? tenantId, CancellationToken cancellationToken = default);
    Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaRequest request, CancellationToken cancellationToken = default);
    Task EsqueciSenhaAsync(EsqueciSenhaRequest request, CancellationToken cancellationToken = default);
    Task RedefinirSenhaAsync(RedefinirSenhaRequest request, CancellationToken cancellationToken = default);
    Task<UsuarioResponse?> ObterUsuarioAtualAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
