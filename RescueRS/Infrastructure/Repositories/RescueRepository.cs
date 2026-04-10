using Microsoft.EntityFrameworkCore;
using RescueRS.Application.Enums;
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

    public async Task<IEnumerable<RescueEntity>> GetRescuesByProximityWithStatus(RescueStatusEnum status, double latitude, double longitude)
    {
        var pageSize = this.pagination.pageSize > 0 ? this.pagination.pageSize : 20;

        double latRad = latitude * Math.PI / 180.0;
        double lonRad = longitude * Math.PI / 180.0;

        return await db.Rescues
            .Where(x => x.Status == status)
            .OrderBy(x =>
                6371000.0 * 2.0 * Math.Atan2(
                    Math.Sqrt(
                        Math.Pow(Math.Sin((x.Latitude * Math.PI / 180.0 - latRad) / 2.0), 2) +
                        Math.Cos(latRad) * Math.Cos(x.Latitude * Math.PI / 180.0) *
                        Math.Pow(Math.Sin((x.Longitude * Math.PI / 180.0 - lonRad) / 2.0), 2)),
                    Math.Sqrt(1.0 -
                        (Math.Pow(Math.Sin((x.Latitude * Math.PI / 180.0 - latRad) / 2.0), 2) +
                        Math.Cos(latRad) * Math.Cos(x.Latitude * Math.PI / 180.0) *
                        Math.Pow(Math.Sin((x.Longitude * Math.PI / 180.0 - lonRad) / 2.0), 2)))))
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<RescueEntity>> GetRescuesWithStatus(RescueStatusEnum status)
    {
        DateTimeOffset? lastDate = await this.db.Rescues
                        .Where(x => x.RescueId == (Guid?)this.pagination.cursor)
                        .Select(x => (DateTimeOffset?)x.RequestDateTime)
                        .FirstOrDefaultAsync();

        return await db.Rescues
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.RequestDateTime)
            .ApplyPagination(this.pagination, x => lastDate == null || x.RequestDateTime < lastDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetRescuesByUserId(UserEntity user) =>
        await this.db.Rescues
            .Where(x => (!user.Rescuer && x.RequestedBy == user.UserId) ||
                        (user.Rescuer && x.ConfirmedBy == user.UserId))
            .Select(x => x.RescueId)
            .ToListAsync();

    public async Task<IEnumerable<RescueEntity>> GetMyRescues(Guid userId, bool rescuer)
    {
        DateTimeOffset? lastDate = await this.db.Rescues
                        .Where(x => x.RescueId == (Guid?)this.pagination.cursor)
                        .Select(x => (DateTimeOffset?)x.RequestDateTime)
                        .FirstOrDefaultAsync();

        return await this.db.Rescues
            .Where(x => (!rescuer && (x.RequestedBy == userId)) ||
                        (rescuer && (x.ConfirmedBy == userId || x.UpdatedBy == userId)))
            .OrderBy(x => x.Status)
                .ThenByDescending(x => x.RequestDateTime)
            .ApplyPagination(this.pagination, x => lastDate == null || x.RequestDateTime < lastDate)
            .ToListAsync();
    }
}