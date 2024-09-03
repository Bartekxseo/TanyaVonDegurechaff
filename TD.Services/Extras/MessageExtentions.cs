using Discord;

namespace TD.Services.Extras
{
    public static class MessageExtentions
    {
        public static bool HasOneOfStringPrefix(this IUserMessage msg, string[] strs, ref int argPos, StringComparison comparisonType = StringComparison.Ordinal)
        {
            string content = msg.Content;
            foreach (var str in strs)
            {
                if (!string.IsNullOrEmpty(content) && content.StartsWith(str, comparisonType))
                {
                    argPos = str.Length;
                    return true;
                }
            }


            return false;
        }

        public static TimeSpan AsTimeSpan(this string input)
        {
            {
                if (string.IsNullOrEmpty(input))
                {
                    return TimeSpan.Zero;
                }

                TimeSpan timeSpan = TimeSpan.Zero;

                int startIndex = 0;
                while (startIndex < input.Length)
                {
                    int endIndex = startIndex + 1;
                    while (endIndex < input.Length && !char.IsLetter(input[endIndex]))
                    {
                        endIndex++;
                    }
                    string valuePart = input.Substring(startIndex, endIndex - startIndex);
                    if (!double.TryParse(valuePart, out double value))
                    {
                        throw new ArgumentException($"Invalid number format for {valuePart}.");
                    }
                    char unit = input[endIndex];
                    timeSpan += unit switch
                    {
                        'd' => TimeSpan.FromDays(value),
                        'h' => TimeSpan.FromHours(value),
                        'm' => TimeSpan.FromMinutes(value),
                        's' => TimeSpan.FromSeconds(value),
                        'w' => TimeSpan.FromDays(value * 7),
                        _ => throw new ArgumentException($"Invalid unit '{unit}'."),
                    };
                    startIndex = endIndex + 1;
                }

                return timeSpan;
            }
        }

        public static string AsString(this TimeSpan timeSpan)
        {
            string formattedString = "";

            if (timeSpan.Days != 0)
            {
                formattedString += $"{timeSpan.Days}d";
            }
            if (timeSpan.Hours != 0)
            {
                formattedString += $"{timeSpan.Hours}h";
            }
            if (timeSpan.Minutes != 0)
            {
                formattedString += $"{timeSpan.Minutes}m";
            }
            if (timeSpan.Seconds != 0)
            {
                formattedString += $"{timeSpan.Seconds}s";
            }
            if (timeSpan.Milliseconds != 0)
            {
                formattedString += $"{timeSpan.Milliseconds}ms";
            }

            return formattedString;
        }
    }
}
