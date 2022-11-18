namespace HTML_Engine_Library
{
    internal static class StringExtensions
    {
        public static string ReplaceFirst(this string input, string replacement, string value)
        {
            var index = input.IndexOf(replacement);
            return input.Remove(index, replacement.Length).Insert(index, value);
        }

        public static (int, int)? FindPar(this string input)
        {
            var first = -1;
            var second = -1;
            var parcount = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '(')
                {
                    if (first == -1)
                        first = i;
                    parcount++;
                }
                else if (input[i] == ')')
                {
                    second = i;
                    parcount--;
                    if (parcount < 0)
                        throw new FormatException("Unpaired parentheses");
                }
            }
            if (first != -1 && second != -1)
                return (first, second);
            return null;
        }
    }
}
