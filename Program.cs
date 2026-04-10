using System;
using System.IO;
using System.Text.RegularExpressions;

namespace LogFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            string pattern = @"\d{2}:\d{2}:\d{2} \d{3}\(\d{4}\).*[^0-9a-fA-F][^0-9a-fA-F](?:\s*[0-9a-fA-F]{1,2}){1}(?:\s[0-9a-fA-F]{1,2}){7}";
            Regex regex = new Regex(pattern);
            string outputFileName = "output.txt";

            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string[] logFiles = Directory.GetFiles(currentDirectory, "*.log");

                Console.WriteLine($"找到 {logFiles.Length} 个 .log 文件，正在处理...");

                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    foreach (string filePath in logFiles)
                    {
                        if (Path.GetFileName(filePath).Equals(outputFileName, StringComparison.OrdinalIgnoreCase))
                            continue;

                        Console.WriteLine($"正在处理: {Path.GetFileName(filePath)}");
                        foreach (string line in File.ReadLines(filePath))
                        {
                            if (regex.IsMatch(line))
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                }
                Console.WriteLine($"处理完成。结果已保存至: {outputFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }
    }
}