namespace Commander.Nginx.Parser;

/// <summary>
/// Represents a token in the nginx configuration file
/// </summary>
/// <param name="Start">The character index the token starts at</param>
/// <param name="End">The character index the token ends at</param>
/// <param name="Before">The value of the token</param>
/// <param name="Value">The character that terminated the token</param>
/// <param name="Type">What terminated the token</param>
public record class Token(
    int Start, 
    int End, 
    string Before, 
    char Value,
    IndicatorType Type)
{
    public override string ToString()
    {
        return $"{Start}-{End} [{Type}::{(int)Value}] {Before}";
    }
}