using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(u => u.UserProfile)
            .Include(u => u.DailyDietPlans)
            .Include(u => u.Devices)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }
}

