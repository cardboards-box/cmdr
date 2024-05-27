namespace Commander.Nginx.Parser.Statements;

/// <summary>
/// Represents a directive statement
/// </summary>
/// <param name="Keyword">The first argument in the statement</param>
/// <param name="Arguments">The rest of the arguments in the statement</param>
public record class Directive(string Keyword, string Arguments) : IStatement;
