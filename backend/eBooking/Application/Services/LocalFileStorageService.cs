using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Application.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private const string HotelsFolder = "hotels";
        private const string UploadsRoot = "uploads";
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        private readonly string _uploadsPath;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IHostEnvironment environment, ILogger<LocalFileStorageService> logger)
        {
            _logger = logger;
            _uploadsPath = Path.Combine(environment.ContentRootPath, UploadsRoot);
            Directory.CreateDirectory(Path.Combine(_uploadsPath, HotelsFolder));
        }

        public async Task<string> SaveHotelImageAsync(int hotelId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new ArgumentException("Dozvoljeni formati slika su JPG, PNG, WEBP i GIF.");

            var safeName = $"hotel_{hotelId}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var relativePath = $"/{UploadsRoot}/{HotelsFolder}/{safeName}";
            var fullPath = Path.Combine(_uploadsPath, HotelsFolder, safeName);

            await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(output, cancellationToken);

            return relativePath;
        }

        public Task DeleteHotelImageAsync(string? imageUrl, CancellationToken cancellationToken = default)
        {
            if (!IsManagedPath(imageUrl))
                return Task.CompletedTask;

            try
            {
                var relative = imageUrl!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_uploadsPath, relative.Replace($"{UploadsRoot}{Path.DirectorySeparatorChar}", string.Empty, StringComparison.OrdinalIgnoreCase));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Brisanje slike nije uspjelo: {ImageUrl}", imageUrl);
            }

            return Task.CompletedTask;
        }

        public bool IsManagedPath(string? imageUrl) =>
            !string.IsNullOrWhiteSpace(imageUrl) &&
            imageUrl.StartsWith($"/{UploadsRoot}/", StringComparison.OrdinalIgnoreCase);
    }
}
