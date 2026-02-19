using NoireLib;
using System;
using System.Text.RegularExpressions;

namespace PuppetMaster_Enhanced;

internal class CommonHelper
{
    /// <summary>
    /// Returns true if the text matches the given regular expression (case-insensitive). If the regexp is null/empty/whitespace, always returns true.
    /// </summary>
    public static bool RegExpMatch(string text, string regexp)
    {
        if (string.IsNullOrWhiteSpace(regexp))
            return true;

        try
        {
            return Regex.IsMatch(text, regexp, RegexOptions.IgnoreCase);
        }
        catch (Exception ex)
        {
            NoireLogger.LogError(ex, $"[PUPPETMASTER] Wrong RegEXP: {regexp}");
            return false;
        }
    }
}
