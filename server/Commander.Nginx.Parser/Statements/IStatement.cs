namespace Commander.Nginx.Parser.Statements;

/// <summary>
/// Represents a directive, block, or comment that can be parsed from an Nginx configuration file
/// </summary>
public interface IStatement
{
    /// <summary>
    /// The keyword that identifies the statement
    /// </summary>
    string Keyword { get; }
}
