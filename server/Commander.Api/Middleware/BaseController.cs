using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;

namespace Commander.Api.Middleware;

[ApiController]
public class BaseController(
    ILogger _logger,
    IDbService _db) : ControllerBase
{
    public RequestValidator Validator => new();

    public IDbService Database => _db;

    public ILogger Logger => _logger;

    [NonAction]
    public Guid? ProfileId()
    {
        if (User is null) return null;

        var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(id) ||
            !Guid.TryParse(id, out var guid)) return null;
        return guid;
    }

    [NonAction]
    public IActionResult Do(Boxed boxed)
    {
        return StatusCode(boxed.Code, boxed);
    }

    [NonAction]
    public Task<IActionResult> Handle<T>(Func<Guid?, Boxed> action, T? body)
    {
        return HandleAsync((f) => Task.FromResult(action(f)), body);
    }

    [NonAction]
    public Task<IActionResult> Handle(Func<Guid?, Boxed> action)
    {
        return Handle((f) => action(f), (object?)null);
    }

    [NonAction]
    public Task<IActionResult> Handle(Func<Boxed> action)
    {
        return Handle((f) => action(), (object?)null);
    }

    [NonAction]
    public Task<IActionResult> HandleAsync(Func<Guid?, Task<Boxed>> action)
    {
        return HandleAsync(action, (object?)null);
    }

    [NonAction]
    public Task<IActionResult> HandleAsync(Func<Task<Boxed>> action)
    {
        return HandleAsync(action, (object?)null);
    }

    [NonAction]
    public Task<IActionResult> HandleAsync<T>(Func<Task<Boxed>> action, T? body)
    {
        return HandleAsync((p) => action(), body);
    }

    [NonAction]
    public async Task<IActionResult> HandleAsync<T>(Func<Guid?, Task<Boxed>> action, T? body)
    {
        var start = DateTime.Now;
        var pid = ProfileId();

        Boxed result;
        Exception? exception = null;
        try
        {
            result = await action(pid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request");
            exception = ex;
            result = Boxed.Exception(ex);
        }
        var url = Request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? Request.GetDisplayUrl();
        var log = new RequestLog
        {
            ProfileId = pid,
            StartTime = start,
            Url = url,
            Code = result.Code,
            Body = body is null ? null : JsonSerializer.Serialize(body),
            StackTrace = exception?.ToString(),
            EndTime = DateTime.Now,
        };
        result.RequestId = await _db.RequestLogs.Insert(log);
        return Do(result);
    }

    [NonAction]
    public Task<IActionResult> AuthorizedAsync(Func<Guid, Task<Boxed>> action)
    {
        return AuthorizedAsync(action, (object?)null);
    }

    [NonAction]
    public Task<IActionResult> AuthorizedAsync<T>(Func<Guid, Task<Boxed>> action, T? body) => HandleAsync(async (pid) =>
    {
        if (pid is null || !pid.HasValue || pid == Guid.Empty)
            return Boxed.Unauthorized();

        return await action(pid.Value);
    }, body);

    [NonAction]
    public Task<IActionResult> RolesAsync<T>(Func<Guid, Task<Boxed>> action, T? body, params string[] roles) => AuthorizedAsync(async (pid) =>
    {
        var hasRole = roles.Any(t => User.IsInRole(t));
        if (!hasRole) return Boxed.Unauthorized("You do not have the required role to access this resource");

        return await action(pid);
    }, body);

    [NonAction]
    public Task<IActionResult> RolesAsync(Func<Guid, Task<Boxed>> action, params string[] roles) => RolesAsync(action, (object?)null, roles);
    
    [NonAction]
    public Task<IActionResult> AdminsAsync(Func<Guid, Task<Boxed>> action) => AdminsAsync(action, (object?)null);

    [NonAction]
    public Task<IActionResult> AdminsAsync<T>(Func<Guid, Task<Boxed>> action, T? body) => RolesAsync(action, body, Role.ADMIN);
    
    [NonAction]
    public Task<IActionResult> ModsAsync(Func<Guid, Task<Boxed>> action) => ModsAsync(action, (object?)null);
    
    [NonAction]
    public Task<IActionResult> ModsAsync<T>(Func<Guid, Task<Boxed>> action, T? body) => RolesAsync(action, body, Role.ADMIN, Role.MODERATOR, Role.AGENT);
    
    [NonAction]
    public Task<IActionResult> BotAsync(Func<Guid, Task<Boxed>> action) => BotAsync(action, (object?)null);

    [NonAction]
    public Task<IActionResult> BotAsync<T>(Func<Guid, Task<Boxed>> action, T? body) => RolesAsync(action, body, Role.ADMIN, Role.AGENT);
}
