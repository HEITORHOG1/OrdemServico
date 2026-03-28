using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

/// <summary>
/// Usuario do ASP.NET Identity. Vinculado ao Domain.Usuario via Usuario.IdentityUserId.
/// </summary>
public sealed class AppIdentityUser : IdentityUser
{
}
