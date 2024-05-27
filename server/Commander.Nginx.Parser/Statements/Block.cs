namespace Commander.Nginx.Parser.Statements;

/// <summary>
/// Represents a block that contains nested statements
/// </summary>
/// <param name="Keyword">The first argument for the block</param>
/// <param name="Arguments">The rest of the arguments for the block</param>
/// <param name="Statements">All of the nested statements</param>
public record class Block(string Keyword, string Arguments, IStatement[] Statements) : IStatement;