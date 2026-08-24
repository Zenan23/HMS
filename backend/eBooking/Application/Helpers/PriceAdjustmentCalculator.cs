using Persistence.Models;

namespace Application.Helpers
{
    public static class PriceAdjustmentCalculator
    {
        public static decimal Apply(decimal basePrice, IEnumerable<PriceAdjustment> adjustments)
        {
            var list = adjustments.ToList();
            if (list.Count == 0)
                return basePrice;

            var cumulative = list.Where(a => a.IsCumulative).ToList();
            var nonCumulative = list.Where(a => !a.IsCumulative).ToList();

            // Kumulativna pravila se uvijek stack-aju (compounding) jedno na drugo.
            decimal result = basePrice;
            foreach (var adj in cumulative)
            {
                result += result * (adj.PercentageModifier / 100m);
            }

            // Nekumulativna pravila se MEĐUSOBNO isključuju — primjenjuje se samo JEDNO.
            // Bira se ono najpovoljnije za gosta (najniža konačna cijena — OrderBy po
            // vrijednosti, ne po apsolutnom iznosu), jer bi npr. nekumulativno poskupljenje
            // od +30% inače "pobijedilo" nekumulativni popust od -10% samo zato što ima veći
            // apsolutni iznos, iako je popust ono što gost treba dobiti. Primjenjuje se NA VRH
            // već kumulativno izračunate cijene (result), ne iznova na basePrice — stari kod je
            // ovdje resetovao na "basePrice + ...", pa je svako aktivno nekumulativno pravilo
            // POTPUNO odbacivalo efekat kumulativnih pravila, čineći konačnu cijenu višom nego
            // što je trebala biti kad su oba tipa pravila aktivna istovremeno.
            if (nonCumulative.Count > 0)
            {
                var best = nonCumulative
                    .OrderBy(a => a.PercentageModifier)
                    .First();
                result += result * (best.PercentageModifier / 100m);
            }

            return result < 0 ? 0 : result;
        }
    }
}
