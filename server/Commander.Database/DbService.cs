using Commander.Database.Services;

namespace Commander.Database;

public interface IDbService
{
    IRoleDbService Roles { get; }

    IRequestLogDbService RequestLogs { get; }
}

public class DbService(
    IRoleDbService _roles,
    IRequestLogDbService _requests) : IDbService
{
    public IRoleDbService Roles => _roles;

    public IRequestLogDbService RequestLogs => _requests;
}
