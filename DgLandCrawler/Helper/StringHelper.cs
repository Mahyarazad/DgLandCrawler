namespace DgLandCrawler.Helper
{
    public static class StringHelper
    {
        public static string CapitalizeWords(string input)
        {
            // Split the input string into words
            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Capitalize each word
            for(int i = 0; i < words.Length; i++)
            {
                // Capitalize first letter, and make the rest of the letters lowercase
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }

            // Join the words back into a single string with spaces
            return string.Join(" ", words);
        }
    }
}
