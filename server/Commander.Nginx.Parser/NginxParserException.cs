namespace Commander.Nginx.Parser;

/// <summary>
/// Represents an exception that occurs during parsing of an Nginx configuration file
/// </summary>
/// <param name="message">The error message</param>
public class NginxParserException(string message) : Exception(message)
{
    /// <summary>
    /// The token that threw the exception
    /// </summary>
    public Token? Token { get; }

    /// <summary>
    /// Represents an exception that occurs during parsing of an Nginx configuration file
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="token">The token that threw the exception</param>
    public NginxParserException(string message, Token token) 
        : this(message + " Token: " + token.ToString())
    {
        Token = token;
    }
}
