using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Shelly_CLI.Utility;


public record LogEntry(
    DateTime Timestamp, 
    string? SectionType, 
    string Content);

public class LogParser
{
    private const string LOG_PATH = "/var/log/shelly.log";
    private static readonly Regex TimestampRegex = 
        new(@"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]", RegexOptions.Compiled);
    private static readonly Regex SectionRegex = 
        new(@":\s*(\w+):?\s*(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public static IEnumerable <LogEntry> ParseStream()
    {
        if (!File.Exists(LOG_PATH))
        {
            Console.Error.WriteLine("Log file not found: " + LOG_PATH);
            yield break; // Doing this instead of throwing an exception to not stop the program from running.
        }

        using var stream =File.OpenRead(LOG_PATH);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.TrimEnd('\r', '\n');

            if (string.IsNullOrEmpty(trimmed)) continue;
            if (trimmed.StartsWith("=")) continue;
            if (!trimmed.StartsWith('[')) continue;
            
            var timeMatch = TimestampRegex.Match(trimmed);
            if (!timeMatch.Success) continue;
            
            string timeStr = timeMatch.Groups[1].Value;
            
            if (!DateTime.TryParseExact(timeStr, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out var timestamp))continue;

            string? type = null;
            string content = trimmed;
            var sectionMatch = SectionRegex.Match(trimmed);
            
            if (SectionRegex.IsMatch(trimmed))
            {
                type = sectionMatch.Groups[1].Value;
                content = sectionMatch.Groups[2].ToString().Trim();
            }
            yield return new LogEntry(timestamp, type, content);
        }
    }

    public static IEnumerable<LogEntry> FindInFile(
        string commandKeyword,
        DateTime startTime,
        DateTime endTime)
    {
        var lowerKeyword = commandKeyword.ToLowerInvariant();

        foreach (var entry in ParseStream())
        {
            if (entry.Timestamp < startTime || entry.Timestamp > endTime) continue;
            
            if (!string.Equals(entry.SectionType, "Command", StringComparison.Ordinal)) continue;

            if (entry.Content.Contains(lowerKeyword))
            {
                yield return entry;
            }
        }
    }
    
}