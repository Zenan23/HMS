using API.Interfaces;
using API.Models;

namespace API.Queries
{
    public class ServiceQueries : IServiceQueries
    {
        private readonly IRepository<Service> _serviceRepository;

        public ServiceQueries(IRepository<Service> serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _serviceRepository.GetByIdAsync(id);
        }
    }
}


