using System.ComponentModel;
using System.Runtime.CompilerServices;
using Web.State;

namespace Web.ViewModels.Foundation;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    private bool _isLoading;
    private bool _isSubmitting;
    private string? _errorMessage;
    private string? _successMessage;
    private ViewState _state;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsLoading
    {
        get => _isLoading;
        protected set => SetProperty(ref _isLoading, value);
    }

    public bool IsSubmitting
    {
        get => _isSubmitting;
        protected set => SetProperty(ref _isSubmitting, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set => SetProperty(ref _errorMessage, value);
    }

    public string? SuccessMessage
    {
        get => _successMessage;
        protected set => SetProperty(ref _successMessage, value);
    }

    public ViewState State
    {
        get => _state;
        protected set => SetProperty(ref _state, value);
    }

    protected void SetLoadingState()
    {
        State = ViewState.Loading;
        IsLoading = true;
        ErrorMessage = null;
    }

    protected void SetSubmittingState()
    {
        State = ViewState.Submitting;
        IsSubmitting = true;
        ErrorMessage = null;
    }

    protected void SetErrorState(string message)
    {
        State = ViewState.Error;
        IsLoading = false;
        IsSubmitting = false;
        ErrorMessage = message;
    }

    protected void SetSuccessState(string? message = null)
    {
        State = ViewState.Success;
        IsLoading = false;
        IsSubmitting = false;
        SuccessMessage = message;
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
