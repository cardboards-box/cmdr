using Commander.Nginx.Parser;

//Look for the nginx.conf file on the desktop
var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "nginx.conf");
//Create a parser from a file
var parser = NginxParser.FromFile(file);
//Parse all of the statements in the file
var statements = parser.Parse();
//Print out all of the statements
Console.WriteLine(statements.PrettyPrint());
