using Radzen;
using Web.State;

namespace Web.Services.Ui;

public sealed class AppNotifier : IAppNotifier
{
    private readonly NotificationService _notificationService;

    public AppNotifier(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void NotifyResult(
        OperationResult result,
        string summary,
        string? successDefault = null,
        string? errorDefault = null,
        bool notifySuccess = true)
    {
        if (result.Succeeded)
        {
            if (!notifySuccess)
            {
                return;
            }

            Success(summary, result.Message ?? successDefault ?? "Operacao concluida com sucesso.");
            return;
        }

        ShowError(summary, result.Message ?? errorDefault ?? "Nao foi possivel concluir a operacao.");
    }

    public void Success(string summary, string detail, int duration = 3000)
        => Notify(NotificationSeverity.Success, summary, detail, duration);

    public void ShowError(string summary, string detail, int duration = 4500)
        => Notify(NotificationSeverity.Error, summary, detail, duration);

    private void Notify(NotificationSeverity severity, string summary, string detail, int duration)
    {
        _notificationService.Notify(new NotificationMessage
        {
            Severity = severity,
            Summary = summary,
            Detail = detail,
            Duration = duration
        });
    }
}
