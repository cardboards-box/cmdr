namespace Commander.Nginx.Parser;

/// <summary>
/// The type of indicator a character represents
/// </summary>
public enum IndicatorType
{
    /// <summary>
    /// White space characters normally used to separate tokens
    /// </summary>
    Whitespace,
    /// <summary>
    /// Newline character normally used to separate lines
    /// </summary>
    NewLine,
    /// <summary>
    /// Normally a semi-colon used to terminate a statement
    /// </summary>
    StatementTerminator,
    /// <summary>
    /// Normally an open curly brace used to start a block
    /// </summary>
    BlockStart,
    /// <summary>
    /// Normally a close curly brace used to end a block
    /// </summary>
    BlockEnd,
    /// <summary>
    /// Normally a hash-bang / pound sign used to start a comment
    /// </summary>
    Comment,
    /// <summary>
    /// Indicates that the end of the file has been reached
    /// </summary>
    EndOfFile,
    /// <summary>
    /// Indicates an escape character
    /// </summary>
    Escape,
    /// <summary>
    /// Indicates that the character should be ignored
    /// </summary>
    Ignore,
}
