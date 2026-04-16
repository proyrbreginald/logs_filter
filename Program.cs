using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;

class Program
{
    private const string AppName = "logs_filter";
    private static readonly string AppVersion =
        typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(Program).Assembly.GetName().Version?.ToString()
        ?? "unknown";

    private static readonly Regex InputFileRegex = new Regex(
        @"^NavigationService\.g3log\..*\.log$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LineRegex = new Regex(
        @"^(\d{2}:\d{2}:\d{2} \d{3}\(\d{4}\)[A-F]/[a-zA-Z_]\w*:)\s*(?:|.*<===|.*===>|.*=|.*:|[a-zA-Z_]\w*(?: [a-zA-Z_]\w*)*)\s*([0-9a-fA-F]{1,2}(?:\s[0-9a-fA-F]{1,2}){7})(?:\s*|\s*,.*|\s*\[(?:(?:0x)?[0-9a-fA-F]{1,2}(?:\s*(?:0x)?[0-9a-fA-F]{1,2})*)\])$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    static void Main(string[] args)
    {
        if (args.Any(IsHelpArg))
        {
            PrintHelp();
            return;
        }

        if (args.Any(IsVersionArg))
        {
            PrintVersion();
            return;
        }

        string[] unknownOptions = args
            .Where(arg => arg.StartsWith('-') && !IsHelpArg(arg) && !IsVersionArg(arg))
            .ToArray();
        if (unknownOptions.Length > 0)
        {
            Console.WriteLine($"Unknown option(s): {string.Join(", ", unknownOptions)}");
            Console.WriteLine();
            PrintHelp();
            return;
        }

        string[] pathArgs = args.Where(arg => !arg.StartsWith('-')).ToArray();
        if (pathArgs.Length > 1)
        {
            Console.WriteLine("Error: Too many directory arguments.");
            Console.WriteLine();
            PrintHelp();
            return;
        }

        string currentDir = pathArgs.Length == 1 ? pathArgs[0] : Environment.CurrentDirectory;
        if (!Directory.Exists(currentDir))
        {
            Console.WriteLine($"Directory does not exist: {currentDir}");
            return;
        }

        string[] files = Directory
            .GetFiles(currentDir, "*.log")
            .Where(path => InputFileRegex.IsMatch(Path.GetFileName(path)))
            .ToArray();

        if (files.Length == 0)
        {
            Console.WriteLine($"No files matched in directory: {currentDir}");
            return;
        }

        foreach (string file in files)
        {
            string inputName = Path.GetFileName(file);
            string outputName = "can_service" + inputName.Substring("NavigationService".Length);
            string outputPath = Path.Combine(currentDir, outputName);

            Console.WriteLine($"Processing: {inputName} -> {outputName}");

            try
            {
                int matchCount = 0;
                using (StreamReader reader = new StreamReader(file))
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Match match = LineRegex.Match(line);
                        if (match.Success)
                        {
                            string prefix = match.Groups[1].Value;
                            string hexGroup = match.Groups[2].Value.Trim();
                            string[] hexArray = hexGroup.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string replacedHex = "[" + string.Join(" ", hexArray.Select(h => "0x" + h.ToUpper())) + "]";

                            writer.WriteLine(prefix + replacedHex);
                            matchCount++;
                        }
                    }
                }
                Console.WriteLine($" -> Completed. {matchCount} lines reformatted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" -> Error processing file: {ex.Message}");
            }
        }
        Console.WriteLine("\nAll tasks finished.");
    }

    private static bool IsHelpArg(string arg)
    {
        return arg == "-h" || arg == "--help" || arg == "/?";
    }

    private static bool IsVersionArg(string arg)
    {
        return arg == "-v" || arg == "--version";
    }

    private static void PrintHelp()
    {
        Console.WriteLine($@"Usage:
  {AppName} [directory] [options]

Arguments:
  directory          Directory containing NavigationService.g3log.*.log files.
                     Defaults to current directory.

Options:
  -h, --help         Show this help message.
  -v, --version      Show application version.");
    }

    private static void PrintVersion()
    {
        Console.WriteLine($"{AppName} {AppVersion}");
    }
}