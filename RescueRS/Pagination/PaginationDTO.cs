namespace ResgateRS.Pagination;

public class PaginationDTO {
    public object? cursor { get; set; } = null!;
    public int page { get; set; } = 0;
    public int pageSize { get; set; } = 20;
}