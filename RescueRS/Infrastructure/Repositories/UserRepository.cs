using Microsoft.EntityFrameworkCore;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;

namespace ResgateRS.Infrastructure.Repositories;

public class UserRepository(ResgateRSDbContext dbContext) : IRepository
{
    private readonly ResgateRSDbContext _db = dbContext;

    public async Task<UserEntity?> GetUser(LoginRequestDTO dto) =>
        await this._db.Users
            .FirstOrDefaultAsync(e => e.Celphone == dto.Celphone && e.Rescuer == dto.Rescuer);

    public async Task<UserEntity> InsertOrUpdate(UserEntity user)
    {
        if (user.UserId == Guid.Empty)
            await this._db.Users.AddAsync(user);
        else
            this._db.Users.Update(user);

        await this._db.SaveChangesAsync();

        return user;
    }
}