using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetUserProfileDetailsByIdAsync(int id);
}

