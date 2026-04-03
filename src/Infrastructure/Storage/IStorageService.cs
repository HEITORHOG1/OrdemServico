namespace Infrastructure.Storage;

public interface IStorageService
{
    Task<string> UploadFileAsync(string fileName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
