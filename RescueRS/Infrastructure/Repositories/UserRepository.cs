using Microsoft.EntityFrameworkCore;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Pagination;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;

namespace ResgateRS.Infrastructure.Repositories;

public class UserRepository(ResgateRSDbContext _dbContext, PaginationDTO _pagination) : BaseRepository(_dbContext, _pagination), IRepository {

    public async Task<UserEntity?> GetUser(LoginRequestDTO dto) =>
        await this.db.Users
            .FirstOrDefaultAsync(e => e.Cellphone == dto.Cellphone && e.Rescuer == dto.Rescuer);

    public async Task<UserEntity> InsertOrUpdate(UserEntity user)
    {
        if (user.UserId == Guid.Empty)
            await this.db.Users.AddAsync(user);
        else
            this.db.Users.Update(user);

        await this.db.SaveChangesAsync();

        return user;
    }
}