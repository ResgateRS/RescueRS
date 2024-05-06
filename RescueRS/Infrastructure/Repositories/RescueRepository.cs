using Microsoft.EntityFrameworkCore;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Infrastructure.Database;
using ResgateRS.Pagination;

namespace ResgateRS.Infrastructure.Repositories;

public class RescueRepository(ResgateRSDbContext _dbContext, PaginationDTO _pagination) : BaseRepository(_dbContext, _pagination), IRepository
{

    public async Task<RescueEntity?> GetRescueById(Guid rescueId) =>
        await db.Rescues
            .FirstOrDefaultAsync(x => x.RescueId == rescueId);

    public async Task InsertOrUpdate(RescueEntity rescue)
    {
        if (rescue.RescueId == Guid.Empty)
            await db.Rescues.AddAsync(rescue);
        else
            db.Rescues.Update(rescue);

        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<RescueEntity>> GetPendingRescues()
    {
        DateTimeOffset? lastDate = await this.db.Rescues
                        .Where(x => x.RescueId == (Guid?)this.pagination.cursor)
                        .Select(x => (DateTimeOffset?)x.RequestDateTime)
                        .FirstOrDefaultAsync();

        return await db.Rescues
            .Where(x => !x.Rescued)
            .OrderBy(x => x.RequestDateTime)
            .ApplyPagination(this.pagination, x => lastDate == null || x.RequestDateTime < lastDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<RescueEntity>> GetPendingRescuesByProximity(double latitude, double longitude)
    {
        double? lastDistance = await this.db.Rescues
                        .Where(x => x.RescueId == (Guid?)this.pagination.cursor)
                        .Select(x => x.GetDistance(latitude, longitude))
                        .FirstOrDefaultAsync();

        return await db.Rescues
            .Where(x => !x.Rescued)
            .ApplyPagination(this.pagination, x => lastDistance == null || x.GetDistance(latitude, longitude) < lastDistance)
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetRescuesByUserId(UserEntity user) =>
        await this.db.Rescues
            .Where(x => (!user.Rescuer && x.RequestedBy == user.UserId) ||
                        (user.Rescuer && x.ConfirmedBy == user.UserId))
            .Select(x => x.RescueId)
            .ToListAsync();

    public async Task<IEnumerable<RescueEntity>> GetMyRescues(int page, int size, Guid userId, bool rescuer) =>
        await this.db.Rescues
            .Where(x => (!rescuer && x.RequestedBy == userId) ||
                        (rescuer && x.ConfirmedBy == userId))
            // .Skip((page - 1) * size)
            .ApplyPagination(this.pagination, x => x.RescueId.CompareTo((Guid?)this.pagination.cursor) > 0)
            // .Take(size)
            .ToListAsync();
}