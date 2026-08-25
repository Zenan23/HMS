namespace Persistence.Models
{
    /// <summary>
    /// Referentna/šifarnik tabela kategorija artikala skladišta (npr. Higijena, Mini bar, Tekstil).
    /// InventoryItem.InventoryItemCategoryId je FK na ovu tabelu — zamjenjuje slobodan tekstualni
    /// unos kategorije (isti obrazac kao ServiceCategory/Service; upute eksplicitno traže FK prema
    /// zasebnoj tabeli, a ne string polje).
    /// </summary>
    public class InventoryItemCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}
