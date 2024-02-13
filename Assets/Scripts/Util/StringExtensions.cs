public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) { return input; }

        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            bool isUpperCase = char.IsUpper(c);

            // Corrected logic to avoid IndexOutOfRangeException
            if (isUpperCase && i > 0 && input[i - 1] != '_' && !(i < 3 && (i == 1 || char.IsUpper(input[i - 1]))))
            {
                builder.Append('_');
            }

            // Always convert to lower case if it's upper case
            builder.Append(isUpperCase ? char.ToLower(c) : c);
        }
        return builder.ToString();
    }
}