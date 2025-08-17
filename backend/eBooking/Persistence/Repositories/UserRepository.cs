using Persistence.Interfaces;
using Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Set<User>()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Set<User>()
                .AnyAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
        }
    }
}
