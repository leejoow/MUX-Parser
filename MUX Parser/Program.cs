using System;
using System.Collections.Generic;

namespace MUX_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = System.IO.File.ReadAllLines("c:\\temp\\muxlog.txt");

            var lineDate = string.Empty;
            var lineData = string.Empty;
            var parsedLines = new List<string>();
            var multiLine = false;

            foreach (var line in lines)
            {
                var workLine = line.Replace("\\0Ah", "");

                lineDate = workLine.Substring(0, 20);
                lineData = workLine.Substring(21);

                var records = lineData.Split("\\0Dh",StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < records.Length; i++)
                {
                    if (i == 0 && multiLine)
                        parsedLines[^1] = parsedLines[^1] + records[i];
                    else
                        parsedLines.Add($"{lineDate} {records[i]}");
                }

                multiLine = !workLine.EndsWith("\\0Dh");
            }

            DecodeAISMessage("B3`nuGP0?85EAJ7IPNlbswd5oP06");

            System.IO.File.WriteAllLines("c:\\temp\\muxlog-fixed.txt", parsedLines);
        }

        static string DecodeAISMessage(string message)
        {
            var chars = message.ToCharArray();
            var payload = new List<int>();

            foreach (var c in chars)
            {
                int decimalValue = (int)c - 48;
                if (decimalValue > 40)
                    decimalValue -= 8;

                payload.Add(decimalValue);
            }

            return string.Empty;
        }
    }
}
