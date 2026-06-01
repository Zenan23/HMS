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

            decimal result = basePrice;
            foreach (var adj in cumulative)
            {
                result += result * (adj.PercentageModifier / 100m);
            }

            if (nonCumulative.Count > 0)
            {
                var best = nonCumulative
                    .OrderByDescending(a => Math.Abs(a.PercentageModifier))
                    .First();
                result = basePrice + basePrice * (best.PercentageModifier / 100m);
            }

            return result < 0 ? 0 : result;
        }
    }
}
