namespace Barbershop.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string? entityId = null,
        string? oldValues = null, string? newValues = null, string? userId = null, string? ip = null);
}
