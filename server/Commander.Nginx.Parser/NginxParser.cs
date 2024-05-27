
namespace Commander.Nginx.Parser;

using Statements;

/// <summary>
/// Represents a parser that can parse an Nginx configuration file
/// </summary>
public interface INginxParser : IDisposable
{
    /// <summary>
    /// Parses all of the statements in the configuration file
    /// </summary>
    /// <returns>All of the statements from the file</returns>
    IEnumerable<IStatement> Parse();
}

/// <summary>
/// Represents a parser that can parse an Nginx configuration file
/// </summary>
/// <param name="_reader">Where to read the config from</param>
public class NginxParser(TextReader _reader) : INginxParser
{
    /// <summary>
    /// The default encoding to use when reading a file
    /// </summary>
    public static Encoding DEFAULT_ENCODING { get; set; } = Encoding.UTF8;

    /// <summary>
    /// A map of characters to the thing they indicate in the file
    /// </summary>
    public Dictionary<char, IndicatorType> CharacterMap { get; } = new() 
    { 
        { ' ', IndicatorType.Whitespace},
        {'\t', IndicatorType.Whitespace},
        {'\n', IndicatorType.NewLine},
        {'\r', IndicatorType.Ignore },
        { ';', IndicatorType.StatementTerminator},
        { '{', IndicatorType.BlockStart},
        { '}', IndicatorType.BlockEnd},
        { '#', IndicatorType.Comment},
        {'\\', IndicatorType.Escape }
    };

    /// <summary>
    /// Parses all of the statements in the configuration file
    /// </summary>
    /// <returns>All of the statements from the file</returns>
    public IEnumerable<IStatement> Parse()
    {
        var tokens = ParseTokens().GetEnumerator();
        while(true)
        {
            var (statement, _) = tokens.ParseStatement(false);
            if (statement == null)
                yield break;

            yield return statement;
        }
    }

    /// <summary>
    /// Parses all of the tokens using the <see cref="CharacterMap"/>
    /// </summary>
    /// <returns>All of the tokens in the file</returns>
    public IEnumerable<Token> ParseTokens()
    {
        int index = -1;
        int start = 0;
        var sb = new StringBuilder();
        var lastWasEscape = false;
        while (true)
        {
            var code = _reader.Read();
            index++;

            if (code == -1)
            {
                var value = sb.ToString();
                yield return new Token(start, index, value, '\0', IndicatorType.EndOfFile);
                yield break;
            }

            var ch = (char)code;
            if (!CharacterMap.TryGetValue(ch, out var type))
            {
                sb.Append(ch);
                lastWasEscape = false;
                continue;
            }

            if (type == IndicatorType.Ignore)
                continue;

            if (lastWasEscape)
            {
                sb.Append(ch);
                lastWasEscape = false;
                continue;
            }

            if (type == IndicatorType.Escape)
            {
                sb.Append(ch);
                lastWasEscape = true;
                continue;
            }

            var output = sb.ToString();
            yield return new Token(start, index, output, ch, type);
            start = index + 1;
            sb.Clear();
            lastWasEscape = false;
        }
    }

    /// <summary>
    /// Disposes of the inbound memory stream
    /// </summary>
    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates a parser from a file
    /// </summary>
    /// <param name="path">The path to the configuration file</param>
    /// <param name="encoding">The encoding to use when reading the file</param>
    /// <returns>The instance of the parser</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file could not be found</exception>
    public static INginxParser FromFile(string path, Encoding? encoding = null)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        var reader = new StreamReader(path, encoding ?? DEFAULT_ENCODING);
        return new NginxParser(reader);
    }

    /// <summary>
    /// Creates a parser from a stream
    /// </summary>
    /// <param name="io">The stream to read the configuration from</param>
    /// <param name="encoding">The encoding to use when reading the configuration</param>
    /// <returns>The instance of the parser</returns>
    public static INginxParser FromStream(Stream io, Encoding? encoding = null)
    {
        var reader = new StreamReader(io, encoding ?? DEFAULT_ENCODING);
        return new NginxParser(reader);
    }

    /// <summary>
    /// Creates a parser from a string
    /// </summary>
    /// <param name="content">The configuration to parse</param>
    /// <param name="encoding">The encoding to use when reading the configuration</param>
    /// <returns>The instance of the parser</returns>
    public static INginxParser FromString(string content, Encoding? encoding = null)
    {
        encoding ??= DEFAULT_ENCODING;
        return FromBytes(encoding.GetBytes(content), encoding);
    }

    /// <summary>
    /// Creates a parser from a set of lines
    /// </summary>
    /// <param name="lines">The lines of the configuration file</param>
    /// <param name="encoding">The encoding to use when reading the configuration</param>
    /// <returns>The instance of the parser</returns>
    public static INginxParser FromLines(string[] lines, Encoding? encoding = null)
    {
        return FromString(string.Join('\n', lines), encoding);
    }

    /// <summary>
    /// Creates a parser from a byte array
    /// </summary>
    /// <param name="bytes">The configuration to parse</param>
    /// <param name="encoding">The encoding to use when reading the configuration</param>
    /// <returns>The instance of the parser</returns>
    public static INginxParser FromBytes(byte[] bytes, Encoding? encoding = null)
    {
        return FromStream(new MemoryStream(bytes), encoding);
    }
}
