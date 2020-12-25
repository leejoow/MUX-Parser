using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MUX_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = System.IO.File.ReadAllText(GetArg(args, "in"));

            var dataLines = new Regex("(?'MessageTime'\\[\\d{1,2}\\-\\d{1,2}\\-\\d{4} \\d{2}:\\d{2}:\\d{2}\\]\\s)?[$!](?<MessageType>[A-Z]{5}).*?\\*[\\dA-F]{2}", RegexOptions.Singleline);
            var cleanup = new Regex("(\\[\\d{1,2}\\-\\d{1,2}\\-\\d{4} \\d{2}:\\d{2}:\\d{2}\\]\\s|\\n|\\r)");
            var payload = new Regex("[!$]AI(?>VDO|VDM),(?<Fragments>\\d),(?<MessageNumber>\\d),(?<Sequence>\\d?),.?,(?<Payload>.+),\\d?\\*[A-F\\d]{2}");
            
            var matchedLines = dataLines.Matches(lines);
            var parsedLines = new List<string>();
            var lastMessageTime = string.Empty;
            var concatenatedPayload = string.Empty;

            foreach (Match m in matchedLines)
            {
                if (!GetArg(args, "filter").Split(",").Contains(m.Groups["MessageType"].Value))
                    continue;

                if (m.Groups["MessageTime"].Value != string.Empty)
                    lastMessageTime = m.Groups["MessageTime"].Value;

                var cleanedUpRow = cleanup.Replace(m.Value, string.Empty);

                if (GetArg(args, "aisdecode") == "true")
                    if (m.Groups["MessageType"].Value == "AIVDO" | m.Groups["MessageType"].Value == "AIVDM")
                    {
                        var a = payload.Match(cleanedUpRow);

                        if (a.Success)
                        {
                            concatenatedPayload += a.Groups["Payload"].Value;

                            if (a.Groups["Fragments"].Value == a.Groups["MessageNumber"].Value)
                            {
                                cleanedUpRow += $"{new AISDecode().DecodeAISPayload(concatenatedPayload)}";
                                concatenatedPayload = string.Empty;
                            }
                        }
                    }

                parsedLines.Add($"{lastMessageTime}{cleanedUpRow}");
            }
           
            System.IO.File.WriteAllLines(GetArg(args, "out"), parsedLines);
        }

        public static string GetArg(string[] args, string key)
        {
            var b = $"-{key}:";

            return args.Where(a => a.StartsWith(b)).FirstOrDefault()?.Replace(b, string.Empty);
        }
    }
}
