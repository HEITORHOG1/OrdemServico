namespace Infrastructure.Storage;

public interface IStorageService
{
    Task<string> UploadFileAsync(string fileName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}

public class AzureBlobStorageService : IStorageService
{
    public Task<string> UploadFileAsync(string fileName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        // Stub inicial
        // No futuro: implementar logica com Azure.Storage.Blobs
        string fakeUrl = $"https://appstorage.blob.core.windows.net/fotos/{fileName}";
        return Task.FromResult(fakeUrl);
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        // Stub inicial
        return Task.CompletedTask;
    }
}
