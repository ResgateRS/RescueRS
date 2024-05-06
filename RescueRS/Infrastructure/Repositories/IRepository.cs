using ResgateRS.Infrastructure.Database;
using ResgateRS.Pagination;
namespace ResgateRS.Infrastructure.Repositories;


public interface IRepository { }

public abstract class BaseRepository {

	protected readonly ResgateRSDbContext db;

	protected readonly PaginationDTO pagination;

	public BaseRepository(ResgateRSDbContext _dbContext, PaginationDTO? _pagination = null) =>
		(db, pagination) = (_dbContext, _pagination ?? new PaginationDTO());
}