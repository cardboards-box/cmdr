namespace Commander.Nginx.Parser.Statements;

/// <summary>
/// Represents the type of a statement
/// </summary>
public enum StatementType
{
    /// <summary>
    /// The statement is a directive
    /// </summary>
    Directive,
    /// <summary>
    /// The statement is a block
    /// </summary>
    Block,
    /// <summary>
    /// The statement is a comment
    /// </summary>
    Comment
}
