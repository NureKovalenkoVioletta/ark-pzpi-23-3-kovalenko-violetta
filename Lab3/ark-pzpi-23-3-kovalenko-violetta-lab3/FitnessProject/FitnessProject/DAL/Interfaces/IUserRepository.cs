using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserDetailsByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
}

