namespace Persistence.Models
{
    /// <summary>
    /// Referentna/šifarnik tabela država. City.CountryId je FK na ovu tabelu —
    /// zamjenjuje slobodan tekstualni unos države na Hotel entitetu.
    /// </summary>
    public class Country : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
