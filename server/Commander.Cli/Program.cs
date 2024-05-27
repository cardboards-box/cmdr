using Commander.Nginx.Parser;
using Commander.Nginx.Parser.Statements;

var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "nginx.conf");
var parser = NginxParser.FromFile(file);

var statements = parser.Parse();

Console.WriteLine(statements.PrettyPrint());
