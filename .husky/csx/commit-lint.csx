// Conventional Commits validator - https://www.conventionalcommits.org/
// Invoked by the commit-msg hook with the path to the commit message file.
//
// Husky.Net .csx scripts run with a MINIMAL set of assembly references:
//   - there are no implicit usings
//   - System.Linq is NOT available (no FirstOrDefault / Any / Where / Select)
// So this script uses plain loops and only System, System.IO and Regex.
using System;
using System.IO;
using System.Text.RegularExpressions;

private var pattern =
    @"^(?:feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(?:\([a-z0-9\-\.\/ ]+\))?!?: .{1,72}$";

// Git generates these itself - do not reject them.
private var exempt = new[] { "Merge ", "Revert ", "fixup!", "squash!" };

if (Args.Count == 0 || !File.Exists(Args[0]))
{
    Console.WriteLine("commit-lint: no commit message file supplied.");
    return 1;
}

// The subject is the first line that is not a comment.
var subject = "";
foreach (var line in File.ReadAllLines(Args[0]))
{
    if (line.StartsWith("#")) continue;
    subject = line.Trim();
    break;
}

foreach (var prefix in exempt)
{
    if (subject.StartsWith(prefix, StringComparison.Ordinal)) return 0;
}

if (Regex.IsMatch(subject, pattern)) return 0;

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine();
Console.WriteLine("  COMMIT REJECTED - message is not a Conventional Commit.");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine($"  You wrote : {subject}");
Console.WriteLine(@"  Expected  : <type>(<scope>): <description>");
Console.WriteLine();
Console.WriteLine("  type    : feat fix docs style refactor perf test build ci chore revert");
Console.WriteLine("  scope   : optional, lowercase, in parentheses");
Console.WriteLine("  subject : 1-72 characters, imperative, no trailing full stop");
Console.WriteLine();
Console.WriteLine("  Examples:");
Console.WriteLine("    feat(catalog): add product search by SKU");
Console.WriteLine("    fix: reject reservation above available stock");
Console.WriteLine();
return 1;