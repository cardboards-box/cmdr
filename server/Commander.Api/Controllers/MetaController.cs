namespace Commander.Api.Controllers;

public class MetaController(
    ILogger<MetaController> _logger,
    IDbService _db) : BaseController(_logger, _db)
{
    [HttpGet, Route("meta/roles"), ProducesArray<Role>]
    public Task<IActionResult> Roles() => HandleAsync(async (_) =>
    {
        var roles = await Database.Roles.Get();
        return Boxed.Ok(roles);
    });

}
