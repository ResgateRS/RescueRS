using Microsoft.EntityFrameworkCore;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;

namespace ResgateRS.Infrastructure.Repositories;

public class UserRepository(ResgateRSDbContext dbContext) : IRepository
{
    private readonly ResgateRSDbContext _db = dbContext;

    public async Task<UserEntity?> GetUser(LoginRequestDTO dto) =>
        await this._db.Users.FirstOrDefaultAsync(e => e.Celphone == dto.Celphone && e.Rescuer == dto.Rescuer);

    internal async Task<UserEntity> InsertOrUpdate(UserEntity user)
    {
        await this._db.Users.AddAsync(user);
        await this._db.SaveChangesAsync();

        return user;
    }
}