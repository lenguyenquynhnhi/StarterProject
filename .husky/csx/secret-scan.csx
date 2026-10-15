// Scans STAGED files for credential-shaped strings before the commit is created.
// Invoked by the pre-commit hook. Husky passes the staged file paths in Args
// (task-runner.json: "${staged}"), so this script never has to call git itself.
//
// Husky.Net .csx scripts run with a MINIMAL set of assembly references:
//   - there are no implicit usings
//   - System.Linq is NOT available (no Any / Where / Select / ToArray)
//   - do not rely on System.Diagnostics.Process either
// So this script uses plain arrays and loops, and only System, System.IO and Regex.
using System;
using System.IO;
using System.Text.RegularExpressions;

private var ruleNames = new[]
{
    "AWS access key id",
    "GitHub personal token",
    "GitHub fine-grained",
    "Slack token",
    "Google API key",
    "Private key block",
    "Connection password",
    "Generic secret literal",
};

private var rulePatterns = new[]
{
    @"AKIA[0-9A-Z]{16}",
    @"gh[pousr]_[A-Za-z0-9]{36,}",
    @"github_pat_[A-Za-z0-9_]{22,}",
    @"xox[baprs]-[A-Za-z0-9-]{10,}",
    @"AIza[0-9A-Za-z\-_]{35}",
    @"-----BEGIN (?:RSA |EC |DSA |OPENSSH |PGP )?PRIVATE KEY-----",
    @"(?i)\b(?:password|pwd)\s*=\s*[^;\s""']{6,}",
    @"(?i)\b(?:api[_-]?key|secret|token|passphrase)\s*[:=]\s*[""'][A-Za-z0-9/+=_\-]{16,}[""']",
};

// Files that are never text.
private var skipExtensions = new[]
{
    ".png", ".jpg", ".jpeg", ".gif", ".ico", ".pdf", ".zip", ".dll", ".exe",
    ".pptx", ".docx", ".xlsx", ".woff", ".woff2", ".ttf", ".snk"
};

// Known-safe values and paths. Each line: <value-or-path>  <reason>  <review date>
private var allowFile = ".husky/secret-allowlist.txt";
private var allowLines = File.Exists(allowFile) ? File.ReadAllLines(allowFile) : new string[0];

bool IsAllowed(string text)
{
    foreach (var raw in allowLines)
    {
        var l = raw.Trim();
        if (l.Length == 0 || l.StartsWith("#")) continue;
        var entry = l.Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        if (entry.Length > 0 && text.IndexOf(entry, StringComparison.OrdinalIgnoreCase) >= 0) return true;
    }
    return false;
}

var findings = "";
var count = 0;
var checkedFiles = 0;

// A scanner that checks nothing must not look like a scanner that found nothing.
if (Args.Count == 0)
{
    Console.WriteLine("  secret-scan: no staged files were passed to the script - nothing was scanned.");
    Console.WriteLine("  (If you expected a scan, check \"--args\", \"${staged}\" in .husky/task-runner.json.)");
    return 0;
}

foreach (var file in Args)
{
    if (!File.Exists(file)) continue;
    if (IsAllowed(file)) continue;

    var skip = false;
    foreach (var ext in skipExtensions)
    {
        if (file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) skip = true;
    }
    if (skip) continue;

    string[] lines;
    try { lines = File.ReadAllLines(file); }
    catch { continue; }                       // unreadable - skip

    checkedFiles++;

    for (var i = 0; i < lines.Length; i++)
    {
        var line = lines[i];
        if (IsAllowed(line)) continue;

        for (var r = 0; r < rulePatterns.Length; r++)
        {
            if (Regex.IsMatch(line, rulePatterns[r]))
            {
                findings += $"    {file}:{i + 1}  {ruleNames[r]}\n";
                count++;
            }
        }
    }
}

if (count == 0)
{
    Console.WriteLine($"  secret-scan: checked {checkedFiles} file(s), no secrets found.");
    return 0;
}

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine();
Console.WriteLine("  COMMIT REJECTED - possible secret in staged changes.");
Console.ResetColor();
Console.WriteLine();
Console.Write(findings);
Console.WriteLine();
Console.WriteLine("  If this is a real credential:");
Console.WriteLine("    1. ROTATE it now. It is compromised the moment it is on disk in a repo.");
Console.WriteLine("    2. Remove it from the code.");
Console.WriteLine("    3. Tell your team in the channel. A silent leak is the only real failure.");
Console.WriteLine();
Console.WriteLine($"  If this is a false positive: add the value or path to {allowFile}");
Console.WriteLine("  with a reason and a review date. Do NOT delete the rule.");
Console.WriteLine();
return 1;