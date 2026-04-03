using Web.State;

namespace Web.Services.Ui;

public interface IAppNotifier
{
    void NotifyResult(
        OperationResult result,
        string summary,
        string? successDefault = null,
        string? errorDefault = null,
        bool notifySuccess = true);

    void Success(string summary, string detail, int duration = 3000);
    void ShowError(string summary, string detail, int duration = 4500);
}
