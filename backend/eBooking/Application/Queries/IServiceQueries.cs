using Persistence.Models;

namespace Application.Queries
{
    public interface IServiceQueries
    {
        Task<Service?> GetByIdAsync(int id);
    }
}


