namespace Project.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = "";
            for (int i = 0; i < input.Length; i++)
            {
                var character = input[i];
                if (char.IsUpper(character) && i > 0)
                {
                    result += "_";
                }
                result += char.ToLower(character);
            }
            return result;
        }
    }
}