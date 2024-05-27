using Commander.Nginx.Parser;
using Commander.Nginx.Parser.Statements;

namespace Commander.Api.Controllers;

public class ParserController(
    ILogger<ParserController> _logger,
    IDbService _db) : BaseController(_logger, _db)
{
    [HttpPost, Route("parse/string/json"), ProducesArray<Statement>]
    public Task<IActionResult> StringJson([FromBody] ParserRequest request) => Handle((_) =>
    {
        using var parser = NginxParser.FromString(request.Config);
        var statements = parser.Parse();
        var safe = statements.ToJsonSafe().ToArray();
        return Boxed.Ok(safe);
    });

    [HttpPost, Route("parse/string/pretty"), ProducesBox<string>]
    public Task<IActionResult> StringPretty([FromBody] ParserRequest request) => Handle((_) =>
    {
        using var parser = NginxParser.FromString(request.Config);
        var statements = parser.Parse();
        var safe = statements.Serialize();
        return Boxed.Ok(safe);
    });

    [HttpPost, Route("parse/string/logical"), ProducesBox<string>]
    public Task<IActionResult> StringLogic([FromBody] ParserRequest request) => Handle((_) =>
    {
        using var parser = NginxParser.FromString(request.Config);
        var statements = parser.Parse();
        var safe = statements.PrettyPrint();
        return Boxed.Ok(safe);
    });

    [HttpPost, Route("parse/file/json"), ProducesArray<Statement>]
    public Task<IActionResult> FileJson(List<IFormFile> files) => Handle((_) =>
    {
        if (files.Count == 0) return Boxed.Bad("No files were uploaded");
        if (files.Count > 1) return Boxed.Bad("Only one file can be uploaded at a time");

        using var stream = files.First().OpenReadStream();

        using var parser = NginxParser.FromStream(stream);
        var statements = parser.Parse();
        var safe = statements.ToJsonSafe().ToArray();
        return Boxed.Ok(safe);
    });

    [HttpPost, Route("parse/file/pretty"), ProducesBox<string>]
    public Task<IActionResult> FilePretty(List<IFormFile> files) => Handle((_) =>
    {
        if (files.Count == 0) return Boxed.Bad("No files were uploaded");
        if (files.Count > 1) return Boxed.Bad("Only one file can be uploaded at a time");

        using var stream = files.First().OpenReadStream();

        using var parser = NginxParser.FromStream(stream);
        var statements = parser.Parse();
        var safe = statements.Serialize();
        return Boxed.Ok(safe);
    });

    [HttpPost, Route("parse/file/logical"), ProducesBox<string>]
    public Task<IActionResult> FileLogical(List<IFormFile> files) => Handle((_) =>
    {
        if (files.Count == 0) return Boxed.Bad("No files were uploaded");
        if (files.Count > 1) return Boxed.Bad("Only one file can be uploaded at a time");

        using var stream = files.First().OpenReadStream();

        using var parser = NginxParser.FromStream(stream);
        var statements = parser.Parse();
        var safe = statements.PrettyPrint();
        return Boxed.Ok(safe);
    });

    [HttpPost, Route("parse/json/pretty"), ProducesBox<string>]
    public Task<IActionResult> JsonPretty([FromBody] Statement[] statements) => Handle((_) =>
    {
        var safe = statements.FromJsonSafe().Serialize();
        return Boxed.Ok(safe);
    });

    [HttpPost, Route("parse/json/logical"), ProducesBox<string>]
    public Task<IActionResult> JsonLogical([FromBody] Statement[] statements) => Handle((_) =>
    {
        var safe = statements.FromJsonSafe().PrettyPrint();
        return Boxed.Ok(safe);
    });

    public record class ParserRequest(
        [property: JsonPropertyName("config")] string Config);
}
