namespace Backend.Services;

public static class PhoneToken
{
    // Preserve the digits supplied by InBody while accepting common international formatting.
    public static bool TryNormalize(string? value, out string token)
    {
        token = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var input = value.Trim();
        if (input.Length > 32 || !char.IsAsciiDigit(input[^1]))
            return false;

        if (input[0] != '+' && input[0] != '(' && !char.IsAsciiDigit(input[0]))
            return false;

        for (var i = 0; i < input.Length; i++)
        {
            var character = input[i];
            if (char.IsAsciiDigit(character))
                continue;
            if (character == '+' && i == 0)
                continue;
            if (char.IsWhiteSpace(character) || character is '-' or '.' or '(' or ')')
                continue;
            return false;
        }

        token = new string(input.Where(char.IsAsciiDigit).ToArray());
        return token.Length is >= 7 and <= 15;
    }
}
