namespace Persistence.Models
{
    /// <summary>
    /// Referentna/šifarnik tabela kategorija hotelskih servisa (npr. Spa, Food, Transport).
    /// Service.ServiceCategoryId je FK na ovu tabelu — zamjenjuje slobodan tekstualni unos
    /// kategorije (upute eksplicitno traže FK prema zasebnoj tabeli, a ne string polje).
    /// </summary>
    public class ServiceCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
