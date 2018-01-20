using System.Text.RegularExpressions;

namespace SlackInteractiveApp
{
    public static class PayloadHelper
    {
        public static string GetPayload(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            var pattern = "payload=(?<payload>(.|\n)*)";
            if (!Regex.IsMatch(message, pattern))
            {
                return null;
            }

            var payload = Regex.Match(message, pattern).Groups["payload"]?.Value;
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            return payload;
        }
    }
}
