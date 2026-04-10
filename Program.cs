using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

class Program
{
    private static readonly Regex InputFileRegex = new Regex(
        @"^NavigationService\.g3log\..*\.log$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LineRegex = new Regex(
        @"^(\d{2}:\d{2}:\d{2} \d{3}\(\d{4}\)[A-F]/[a-zA-Z_]\w*:)\s*(?:|.*<===|.*===>|.*=|.*:|[a-zA-Z_]\w*(?: [a-zA-Z_]\w*)*)\s*([0-9a-fA-F]{1,2}(?:\s[0-9a-fA-F]{1,2}){7})(?:\s*|\s*,.*|\s*\[(?:(?:0x)?[0-9a-fA-F]{1,2}(?:\s*(?:0x)?[0-9a-fA-F]{1,2})*)\])$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    static void Main(string[] args)
    {
        string currentDir = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
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
}