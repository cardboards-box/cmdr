namespace Commander.Nginx.Parser.Statements;

/// <summary>
/// JSON safe class for representing a directive, block, or comment that can be parsed from an Nginx configuration file
/// </summary>
/// <param name="Keyword">The keyword for the statement</param>
/// <param name="Arguments">The value for the statement</param>
/// <param name="Statements">Any nested statements</param>
/// <param name="Type">The type of statement</param>
public record class Statement(
    [property: JsonPropertyName("keyword")] string Keyword, 
    [property: JsonPropertyName("arguments")] string Arguments, 
    [property: JsonPropertyName("statements")] Statement[] Statements, 
    [property: JsonPropertyName("type")] StatementType Type)
{
    /// <summary>
    /// Converts a directive to a statement
    /// </summary>
    /// <param name="directive">The directive to convert</param>
    public static implicit operator Statement(Directive directive)
    {
        return new Statement(directive.Keyword, directive.Arguments, [], StatementType.Directive);
    }

    /// <summary>
    /// Converts a statement to a directive
    /// </summary>
    /// <param name="statement">The statement to convert</param>
    public static implicit operator Directive(Statement statement)
    {
        if (statement.Type != StatementType.Directive)
            throw new InvalidCastException("Statement is not a directive");

        return new Directive(statement.Keyword, statement.Arguments);
    }

    /// <summary>
    /// Converts a comment to a statement
    /// </summary>
    /// <param name="comment">The comment to convert</param>
    public static implicit operator Statement(Comment comment)
    {
        return new Statement(string.Empty, comment.Text, [], StatementType.Comment);
    }

    /// <summary>
    /// Converts a statement to a comment
    /// </summary>
    /// <param name="statement">The statement to convert</param>
    public static implicit operator Comment(Statement statement)
    {
        if (statement.Type != StatementType.Comment)
            throw new InvalidCastException("Statement is not a comment");

        return new Comment(statement.Arguments);
    }

    /// <summary>
    /// Converts a block to a statement
    /// </summary>
    /// <param name="block">The block to convert</param>
    public static implicit operator Statement(Block block)
    {
        return new Statement(
            block.Keyword, 
            block.Arguments, 
            [..block.Statements.Select(FromStatement)], StatementType.Block);
    }

    /// <summary>
    /// Converts a statement to a block
    /// </summary>
    /// <param name="statement">The statement to convert</param>
    public static implicit operator Block(Statement statement)
    {
        if (statement.Type != StatementType.Block)
            throw new InvalidCastException("Statement is not a block");

        return new Block(
            statement.Keyword, 
            statement.Arguments, 
            [..statement.Statements.Select(t => t.ToStatement())]);
    }

    /// <summary>
    /// Converts the current statement to it's <see cref="IStatement"/> equivalent
    /// </summary>
    /// <returns>The <see cref="IStatement"/> equivalent</returns>
    /// <exception cref="InvalidOperationException">Thrown if the type is invalid</exception>
    public IStatement ToStatement()
    {
        return Type switch
        {
            StatementType.Directive => (Directive)this,
            StatementType.Block => (Block)this,
            StatementType.Comment => (Comment)this,
            _ => throw new InvalidOperationException("Unknown statement type"),
        };
    }

    /// <summary>
    /// Converts an <see cref="IStatement"/> to a <see cref="Statement"/>
    /// </summary>
    /// <param name="statement">The statement to convert</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if the type is invalid</exception>
    public static Statement FromStatement(IStatement statement)
    {
        return statement switch
        {
            Directive directive => directive,
            Block block => block,
            Comment comment => comment,
            _ => throw new InvalidOperationException("Unknown statement type"),
        };
    }
}
