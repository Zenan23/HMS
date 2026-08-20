using System.Linq.Expressions;

namespace Persistence.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Prava DB-level paginacija (EF Skip/Take) — za razliku od učitavanja cijele tabele pa
        /// rezanja u memoriji. Koristiti za sve liste koje mogu narasti (uputa: "Paginacija je
        /// obavezna na svakom list endpointu").
        /// </summary>
        Task<IEnumerable<T>> GetPagedAsync(int skip, int take);

    }
}
