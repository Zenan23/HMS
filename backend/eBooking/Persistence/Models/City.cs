namespace Persistence.Models
{
    /// <summary>
    /// Referentna/šifarnik tabela gradova. Hotel.CityId je FK na ovu tabelu —
    /// zamjenjuje slobodan tekstualni unos grada (upute eksplicitno traže dropdown
    /// popunjen iz baze umjesto textbox-a za gradove).
    /// </summary>
    public class City : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public Country? Country { get; set; }
        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    }
}
