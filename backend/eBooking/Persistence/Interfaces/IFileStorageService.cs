namespace Persistence.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveHotelImageAsync(int hotelId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
        Task DeleteHotelImageAsync(string? imageUrl, CancellationToken cancellationToken = default);
        bool IsManagedPath(string? imageUrl);
    }
}
