using Microsoft.EntityFrameworkCore;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Infrastructure.Database;

namespace ResgateRS.Infrastructure.Repositories;

public class RescueRepository(ResgateRSDbContext dbContext) : IRepository
{
    private readonly ResgateRSDbContext _db = dbContext;


    public async Task<RescueEntity?> GetRescueById(Guid rescueId) =>
        await _db.Rescues
            .FirstOrDefaultAsync(x => x.RescueId == rescueId);

    public async Task InsertOrUpdate(RescueEntity rescue)
    {
        if (rescue.RescueId == Guid.Empty)
            await _db.Rescues.AddAsync(rescue);
        else
            _db.Rescues.Update(rescue);

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<RescueEntity>> GetPendingRescues(int page, int size) =>
        await _db.Rescues
            .Where(x => !x.Rescued)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

    public async Task<IEnumerable<Guid>> GetRescuesByUserId(UserEntity user) =>
        await this._db.Rescues
            .Where(x => (!user.Rescuer && x.RequestedBy == user.UserId) ||
                        (user.Rescuer && x.ConfirmedBy == user.UserId))
            .Select(x => x.RescueId)
            .ToListAsync();

    internal async Task<IEnumerable<RescueEntity>> GetMyRescues(int page, int size, Guid userId, bool rescuer) =>
        await this._db.Rescues
            .Where(x => (!rescuer && x.RequestedBy == userId) ||
                        (rescuer && x.ConfirmedBy == userId))
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
}