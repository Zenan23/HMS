namespace Persistence.Models
{
    /// <summary>
    /// Referentni artikal skladišta (npr. sapun, peškiri, mini bar napici).
    /// InventoryTransaction se veže na ovu tabelu preko InventoryItemId (FK),
    /// umjesto da je to "goli" broj bez referencijalnog integriteta.
    /// </summary>
    public class InventoryItem : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int MinimumStockLevel { get; set; }
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
    }
}
