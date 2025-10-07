namespace FikaWebApp.Components;

public static class Statics
{
    public static bool IsValidMongoId(string input)
    {
        var span = input.AsSpan();

        if (span.Length != 24)
        {
            return false;
        }

        for (var i = 0; i < 24; i++)
        {
            var c = span[i];
            var isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

            if (!isHex)
            {
                return false;
            }
        }

        return true;
    }

    public static string? ValidateMongoId(string input)
    {
        var span = input.AsSpan();

        if (span.Length != 24)
        {
            return "The MongoId is too short! Must be 24 characters.";
        }

        for (var i = 0; i < 24; i++)
        {
            var c = span[i];
            var isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

            if (!isHex)
            {
                return "Input is not a valid MongoId.";
            }
        }

        return null;
    }
}
