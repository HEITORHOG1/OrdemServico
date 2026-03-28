using System.Net;
using System.Text.Json;

namespace Web.Services.Api;

public sealed class ApiErrorParser : IApiErrorParser
{
    public async Task<ApiError> ParseAsync(HttpStatusCode statusCode, HttpContent? content, CancellationToken cancellationToken = default)
    {
        if (content is null)
        {
            return new ApiError(GetStatusFallbackMessage(statusCode));
        }

        var body = await content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return new ApiError(GetStatusFallbackMessage(statusCode));
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var title = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;
            var detail = root.TryGetProperty("detail", out var detailEl) ? detailEl.GetString() : null;

            var message = !string.IsNullOrWhiteSpace(detail)
                ? detail!
                : !string.IsNullOrWhiteSpace(title)
                    ? title!
                    : GetStatusFallbackMessage(statusCode);

            if (!root.TryGetProperty("errors", out var errorsEl) || errorsEl.ValueKind != JsonValueKind.Object)
            {
                return new ApiError(message);
            }

            var validationErrors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var errorProperty in errorsEl.EnumerateObject())
            {
                if (errorProperty.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var messages = errorProperty.Value
                    .EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .ToArray();

                if (messages.Length > 0)
                {
                    validationErrors[errorProperty.Name] = messages;
                }
            }

            return validationErrors.Count == 0
                ? new ApiError(message)
                : new ApiError(message, validationErrors);
        }
        catch (JsonException)
        {
            return new ApiError(GetStatusFallbackMessage(statusCode));
        }
    }

    private static string GetStatusFallbackMessage(HttpStatusCode statusCode)
        => statusCode switch
        {
            HttpStatusCode.BadRequest => "A requisicao enviada e invalida. Revise os dados e tente novamente.",
            HttpStatusCode.Unauthorized => "Sua sessao nao esta autorizada para esta operacao.",
            HttpStatusCode.Forbidden => "Voce nao possui permissao para executar esta operacao.",
            HttpStatusCode.NotFound => "O recurso solicitado nao foi encontrado.",
            HttpStatusCode.UnprocessableEntity => "A API recusou os dados informados. Revise os campos e tente novamente.",
            HttpStatusCode.RequestTimeout => "A API demorou para responder. Tente novamente em instantes.",
            HttpStatusCode.Conflict => "A operacao entrou em conflito com o estado atual dos dados. Atualize a tela e tente novamente.",
            HttpStatusCode.TooManyRequests => "Muitas requisicoes em pouco tempo. Aguarde alguns segundos e tente novamente.",
            HttpStatusCode.ServiceUnavailable => "A API esta indisponivel no momento. Verifique a conexao e tente novamente.",
            HttpStatusCode.GatewayTimeout => "Tempo de resposta da API excedido. Tente novamente.",
            _ when (int)statusCode >= 500 => "O servidor encontrou um erro inesperado. Tente novamente em instantes.",
            _ => $"A API retornou erro {(int)statusCode}."
        };
}
