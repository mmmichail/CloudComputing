using System.Text.RegularExpressions;

namespace CloudComputing.Services
{
    public class RedactionService: IRedactionService
    {
        public string RedactSensitiveInformation(string text)
        {
            var patterns = new Dictionary<string, string>
            {
                { @"\b\d{1,5}\s[A-Za-z0-9\s]+(?:,\s?[A-Za-z\s]+)?(?:,\s?[A-Za-z]{2}\s?\d{5})?\b", "*" },
                { @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", "*" },
                { @"\b[A-Z][a-z]+ [A-Z][a-z]+\b", "*" }
            };

            foreach (var pattern in patterns)
            {
                text = Regex.Replace(text, pattern.Key, pattern.Value);
            }

            return text;
        }
    }
}
