using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserProfile?> GetUserProfileDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(up => up.User)
            .FirstOrDefaultAsync(up => up.ProfileId == id);
    }
}

