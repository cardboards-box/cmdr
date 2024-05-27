using Commander.Nginx.Parser;

var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
//Look for the nginx.conf file on the desktop
var file = Path.Combine(desktop, "nginx.conf");
//Create a parser from a file
using var parser = NginxParser.FromFile(file);
//Parse all of the statements in the file
var statements = parser.Parse().ToArray();

//Print out the statements in a logical format (with indices)
using (var io = File.CreateText(Path.Combine(desktop, "nginx-pretty.text")))
    await io.WriteAsync(statements.PrettyPrint());

//Print out the statements in a JSON format
using (var io = File.Create(Path.Combine(desktop, "nginx.json")))
{
    var safe = statements.ToJsonSafe().ToArray();
    var opts = new JsonSerializerOptions { WriteIndented = true };
    await JsonSerializer.SerializeAsync(io, safe, opts);
}

//Print out the statements in a format that can be read by nginx
using (var io = File.CreateText(Path.Combine(desktop, "nginx-formatted.conf")))
    await io.WriteAsync(statements.Serialize());