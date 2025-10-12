using System.Text.RegularExpressions;

namespace Primes1;

public class RegexBenchmark : IBenchmark
{
    public class RegexResult
    {
        public int TextLength { get; set; }
        public int Operations { get; set; }
        public List<string> Emails { get; set; } = [];
        public List<string> Urls { get; set; } = [];
        public List<string> Phones { get; set; } = [];
        public List<string> Dates { get; set; } = [];
        public List<string> Ips { get; set; } = [];
        public List<string> Hashtags { get; set; } = [];
        public List<string> HtmlTags { get; set; } = [];
        public int Replacements { get; set; }
        public int WordCount { get; set; }
        public int TotalMatches { get; set; }
    }

    private static readonly Regex EmailRegex = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"https?://[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}[^\s]*", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"\+?1?\s*\(?[0-9]{3}\)?[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}", RegexOptions.Compiled);
    private static readonly Regex DateRegex = new(@"\d{4}-\d{2}-\d{2}", RegexOptions.Compiled);
    private static readonly Regex IpRegex = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
    private static readonly Regex HashtagRegex = new(@"#[a-zA-Z0-9_]+", RegexOptions.Compiled);
    private static readonly Regex HtmlTagRegex = new(@"<([a-z]+)([^>]*)>(.*?)</\1>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ReplaceRegex = new(@"\b(test|demo|sample)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex EmailValidationRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

    public object Execute(int scale)
    {
        // Generate text data to process
        var text = GenerateText(scale);

        var results = new RegexResult
        {
            TextLength = text.Length,
            Operations = 0
        };

        // Email extraction
        results.Emails = EmailRegex.Matches(text).Select(m => m.Value).ToList();
        results.Operations++;

        // URL extraction
        results.Urls = UrlRegex.Matches(text).Select(m => m.Value).ToList();
        results.Operations++;

        // Phone number extraction (various formats)
        results.Phones = PhoneRegex.Matches(text).Select(m => m.Value).ToList();
        results.Operations++;

        // Date extraction (YYYY-MM-DD format)
        results.Dates = DateRegex.Matches(text).Select(m => m.Value).ToList();
        results.Operations++;

        // IP address extraction
        results.Ips = IpRegex.Matches(text).Select(m => m.Value).ToList();
        results.Operations++;

        // Hashtag extraction
        results.Hashtags = HashtagRegex.Matches(text).Select(m => m.Value).ToList();
        results.Operations++;

        // Complex pattern: HTML tags
        results.HtmlTags = HtmlTagRegex.Matches(text).Select(m => m.Value).ToList();
        results.Operations++;

        // Word replacement (multiple operations)
        var modifiedText = ReplaceRegex.Replace(text, "[REDACTED]");
        results.Replacements = modifiedText.Split("[REDACTED]").Length - 1;
        results.Operations++;

        // Split by whitespace and count words
        var words = WhitespaceRegex.Split(text).Where(w => !string.IsNullOrEmpty(w)).ToList();
        results.WordCount = words.Count;
        results.Operations++;

        // Complex validation patterns
        foreach (var email in results.Emails)
        {
            EmailValidationRegex.IsMatch(email);
            results.Operations++;
        }

        results.TotalMatches = results.Emails.Count + results.Urls.Count +
                               results.Phones.Count + results.Dates.Count +
                               results.Ips.Count + results.Hashtags.Count +
                               results.HtmlTags.Count;

        return results;
    }

    private string GenerateText(int scale)
    {
        var textParts = new List<string>(scale);

        for (int i = 0; i < scale; i++)
        {
            var daysAgo = i;
            var daysAgo2 = Math.Max(0, i - 7);
            textParts.Add(string.Format(
                "Hello, this is a test message from user{0}@example.com. " +
                "You can visit our website at https://example{1}.com/path/to/resource. " +
                "Call us at +1 (555) {2:000}-{3:0000} or on {4:000}.{5:000}.{6:0000}. " +
                "Event date: {7}. Server IP: 192.168.{8}.{9}. " +
                "Tags: #test{10} #demo #sample{11} #csharp #performance. " +
                "HTML content: <div class='container'><p>Content {12}</p></div>. " +
                "Contact: contact{13}@domain{14}.org or admin{15}@site{16}.net. " +
                "Visit http://test{17}.example.com for more information. " +
                "Phone: 555-{18:000}-{19:0000}. Date range: {20} to {21}. ",
                i,
                i % 100,
                i % 1000,
                i % 10000,
                i % 1000,
                i % 1000,
                i % 10000,
                DateTime.Now.AddDays(-daysAgo).ToString("yyyy-MM-dd"),
                i % 256,
                (i + 1) % 256,
                i,
                i,
                i,
                i,
                i % 50,
                i,
                i % 100,
                i,
                i % 1000,
                i % 10000,
                DateTime.Now.AddDays(-daysAgo).ToString("yyyy-MM-dd"),
                DateTime.Now.AddDays(-daysAgo2).ToString("yyyy-MM-dd")
            ));
        }

        return string.Join("\n", textParts);
    }

    public int GetMetric(object result)
    {
        return ((RegexResult)result).TotalMatches;
    }

    public string GetSample(object result)
    {
        var r = (RegexResult)result;
        return $"Text length: {r.TextLength:N0} bytes\n" +
               $"Total regex operations: {r.Operations}\n" +
               $"Emails found: {r.Emails.Count}\n" +
               $"URLs found: {r.Urls.Count}\n" +
               $"Phone numbers found: {r.Phones.Count}\n" +
               $"Dates found: {r.Dates.Count}\n" +
               $"IP addresses found: {r.Ips.Count}\n" +
               $"Hashtags found: {r.Hashtags.Count}\n" +
               $"HTML tags found: {r.HtmlTags.Count}\n" +
               $"Words replaced: {r.Replacements}\n" +
               $"Total words: {r.WordCount}\n" +
               $"Sample emails: {string.Join(", ", r.Emails.Take(5))}\n" +
               $"Sample URLs: {string.Join(", ", r.Urls.Take(5))}";
    }

    public string GetName() => "Regex Pattern Matching";

    public string GetScaleUnit() => "text entries";
}

