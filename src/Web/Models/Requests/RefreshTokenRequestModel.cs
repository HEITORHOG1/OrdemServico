namespace Web.Models.Requests;

public sealed record RefreshTokenRequestModel(
    string AccessToken,
    string RefreshToken
);
