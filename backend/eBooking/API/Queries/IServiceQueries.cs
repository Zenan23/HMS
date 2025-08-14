using API.Models;

namespace API.Queries
{
    public interface IServiceQueries
    {
        Task<Service?> GetByIdAsync(int id);
    }
}


