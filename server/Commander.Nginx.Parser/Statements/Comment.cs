namespace Commander.Nginx.Parser.Statements;

/// <summary>
/// Represents a comment statement
/// </summary>
/// <param name="text"></param>
public class Comment(string text) : IStatement
{
    /// <summary>
    /// The comment character
    /// </summary>
    public string Keyword { get; } = "#";

    /// <summary>
    /// The text of the comment
    /// </summary>
    public string Text { get; } = text;
}
