namespace ResgateRS.Domain.Application.Entities;

public class AuditLogEntity
{
    public Guid AuditLogId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = null!;
    public Guid? EntityId { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
