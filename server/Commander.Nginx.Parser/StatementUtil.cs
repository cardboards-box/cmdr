namespace Commander.Nginx.Parser;

using Commander.Nginx.Parser.Statements;
using Tokens = IEnumerator<Token>;
using IStatements = IEnumerable<Statements.IStatement>;

/// <summary>
/// A collection of useful extensions for parsing statements
/// </summary>
public static class StatementUtil
{
    /// <summary>
    /// Skips all tokens until a token of the specified type is found
    /// </summary>
    /// <param name="tokens">The token enumerator</param>
    /// <param name="types">The types to stop at</param>
    /// <returns>The token that was found matching the types</returns>
    public static Token? SkipUntil(this Tokens tokens, params IndicatorType[] types)
    {
        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            if (token.Type == IndicatorType.EndOfFile || types.Contains(token.Type)) return token;
        }

        return null;
    }

    /// <summary>
    /// Skips all tokens until a token of the specified type is found, ignoring empty whitespace tokens
    /// </summary>
    /// <param name="tokens">The token enumerator</param>
    /// <param name="types">The types to stop at</param>
    /// <returns>The token that was found matching the types</returns>
    public static Token? SkipUntilIgnoreEmpty(this Tokens tokens, params IndicatorType[] types)
    {
        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            if (token.Type == IndicatorType.EndOfFile && string.IsNullOrEmpty(token.Before)) return null;
            if (token.Type == IndicatorType.EndOfFile) return token;
            if (token.Type == IndicatorType.Whitespace && string.IsNullOrWhiteSpace(token.Before)) continue;
            if (types.Contains(token.Type)) return token;
        }

        return null;
    }

    /// <summary>
    /// Returns all of the tokens until a token of the specified type is found
    /// </summary>
    /// <param name="tokens">The token enumerator</param>
    /// <param name="type">The types stop at</param>
    /// <returns>All of the tokens that were found</returns>
    public static IEnumerable<Token> GetUntil(this Tokens tokens, params IndicatorType[] type)
    {
        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            yield return token;
            if (token.Type == IndicatorType.EndOfFile || type.Contains(token.Type)) yield break;
        }
    }

    /// <summary>
    /// Concatenates all of the tokens into a single string
    /// </summary>
    /// <param name="tokens">The tokens to combine</param>
    /// <returns>The concatenated string</returns>
    public static string Concat(this IEnumerable<Token> tokens)
    {
        var allTokens = tokens.ToArray();

        var sb = new StringBuilder();
        for(var i = 0; i < allTokens.Length; i++)
        {
            sb.Append(allTokens[i].Before);

            if (i >= allTokens.Length - 1) continue;
                
            sb.Append(allTokens[i].Value);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Parses a statement from the token enumerator
    /// </summary>
    /// <param name="tokens">The tokens to parse through</param>
    /// <param name="inBlock">Whether or not the current statement is nested within another block</param>
    /// <returns>The statement and whether or not it ended with a <see cref="IndicatorType.BlockEnd"/></returns>
    /// <exception cref="NginxParserException">Thrown if parser state becomes invalid</exception>
    public static (IStatement? statement, bool blockEnd) ParseStatement(this Tokens tokens, bool inBlock)
    {
        //Get the next keyword token
        var keyOrComment = tokens.SkipUntilIgnoreEmpty(
            IndicatorType.Whitespace,
            IndicatorType.Comment,
            IndicatorType.StatementTerminator,
            IndicatorType.BlockStart,
            IndicatorType.BlockEnd);
        //Token null? return null
        if (keyOrComment == null) return (null, false);

        //Token is a comment? return a comment
        if (keyOrComment.Type == IndicatorType.Comment)
        {
            //If stuff is before the hash-bang, throw an error because that shit isn't kosher.
            if (!string.IsNullOrWhiteSpace(keyOrComment.Before))
                throw new NginxParserException("Unterminated statement before comment", keyOrComment);

            var line = tokens.GetUntil(IndicatorType.NewLine).Concat();
            return (new Comment(line), false);
        }

        //Token keyword is a complete statement? return a directive
        if (keyOrComment.Type == IndicatorType.StatementTerminator)
            return (new Directive(keyOrComment.Before, string.Empty), false);

        //EoF reached
        if (keyOrComment.Type == IndicatorType.EndOfFile)
        {
            //validate there isn't an unterminated expression
            if (!string.IsNullOrWhiteSpace(keyOrComment.Before))
                throw new NginxParserException("Unterminated statement before EOF", keyOrComment);

            return (null, false);
        }

        //Starting a new block without arguments
        if (keyOrComment.Type == IndicatorType.BlockStart)
            return (ParseBlock(tokens, keyOrComment.Before, string.Empty), false);

        //Ending previous block
        if (keyOrComment.Type == IndicatorType.BlockEnd)
        {
            //In a block? escape the block
            if (inBlock) return (null, true);
            //Not in a block? throw an error
            throw new NginxParserException("Unexpected block end", keyOrComment);
        }

        //Get the arguments for the statement and continue parsing
        var keyword = keyOrComment.Before;
        var args = tokens.GetUntil(IndicatorType.StatementTerminator, IndicatorType.BlockStart).ToArray();
        var arguments = args.Concat().Trim();
        //Get the last argument token
        var last = args.LastOrDefault() ?? throw new NginxParserException("Unexpected end of stream - No last argument for statement");

        if (last.Type == IndicatorType.EndOfFile)
            throw new NginxParserException("Unterminated statement before EOF", last);

        //If the last argument is a block start, parse the block
        if (last.Type == IndicatorType.BlockStart)
            return (ParseBlock(tokens, keyword, arguments), false);

        //If the last argument is a statement terminator, return a directive
        return (new Directive(keyword, arguments), false);
    }

    /// <summary>
    /// Parses a block statements
    /// </summary>
    /// <param name="tokens">The tokens to parse through</param>
    /// <param name="keyword">The keyword before the block</param>
    /// <param name="arguments">The arguments for the block</param>
    /// <returns>The statement and all of it's nested statements</returns>
    /// <exception cref="NginxParserException">Thrown if parser state becomes invalid</exception>
    public static IStatement? ParseBlock(this Tokens tokens, string keyword, string arguments)
    {
        var statements = new List<IStatement>();

        while (true)
        {
            var (statement, blockEnd) = ParseStatement(tokens, true);
            if (statement is not null)
                statements.Add(statement);

            if (blockEnd || statement is null) break;
        }

        return new Block(keyword, arguments, [.. statements]);
    }

    /// <summary>
    /// Prints the statements in an easy to read format
    /// </summary>
    /// <param name="statements">The statements to print</param>
    /// <param name="level">The level to offset each block at</param>
    /// <param name="buff">The characters to use to show a new block</param>
    /// <returns></returns>
    public static string PrettyPrint(this IStatements statements, int level = 0, string buff = "\t")
    {
        var sb = new StringBuilder();
        var buffer = string.Join("", Enumerable.Repeat(buff, level));

        foreach (var statement in statements)
        {
            switch(statement)
            {
                case Comment comment:
                    sb.AppendLine($"{buffer}[CMT] {comment.Text}");
                    break;
                case Directive directive:
                    sb.AppendLine($"{buffer}[DIR] `{directive.Keyword}` - \"{directive.Arguments}\"");
                    break;
                case Block block:
                    sb.AppendLine($"{buffer}[BLK] `{block.Keyword}` - \"{block.Arguments}\"");
                    sb.AppendLine(block.Statements.PrettyPrint(level + 1, buff));
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a collection of statements to an nginx configuration string
    /// </summary>
    /// <param name="statements">The statements to serialize</param>
    /// <param name="level">The level to offset each block at</param>
    /// <param name="buff">The characters to use to show a new block</param>
    /// <returns>The NGINX config string</returns>
    public static string Serialize(this IStatements statements, int level = 0, string buff = "\t")
    {
        var sb = new StringBuilder();
        var buffer = string.Join("", Enumerable.Repeat(buff, level));

        foreach (var statement in statements)
        {
            switch (statement)
            {
                case Comment comment:
                    sb.AppendLine($"{buffer}#{comment.Text}");
                    break;
                case Directive directive:
                    var args = string.IsNullOrWhiteSpace(directive.Arguments) ? "" : $" {directive.Arguments}";
                    sb.AppendLine($"{buffer}{directive.Keyword}{args};");
                    break;
                case Block block:
                    var bargs = string.IsNullOrWhiteSpace(block.Arguments) ? "" : $" {block.Arguments}";
                    sb.AppendLine();
                    sb.AppendLine($"{buffer}{block.Keyword}{bargs} {{");
                    sb.AppendLine(block.Statements.Serialize(level + 1, buff).TrimEnd());
                    sb.AppendLine($"{buffer}}}");
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts the given JSON safe statements into a collection of <see cref="IStatement"/>s
    /// </summary>
    /// <param name="statements">The statements to convert</param>
    /// <returns>The converted statements</returns>
    public static IStatements FromJsonSafe(this IEnumerable<Statement> statements)
    {
        return statements.Select(t => t.ToStatement());
    }

    /// <summary>
    /// Converts the given <see cref="IStatements"/> into a collection of JSON safe statements
    /// </summary>
    /// <param name="statements">The statements to convert</param>
    /// <returns>The converted statements</returns>
    public static IEnumerable<Statement> ToJsonSafe(this IStatements statements)
    {
        return statements.Select(Statement.FromStatement);
    }
}
