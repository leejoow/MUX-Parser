using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MUX_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = System.IO.File.ReadAllText("c:\\temp\\muxlog.txt");

            var dataLines = new Regex("(?'MessageTime'\\[\\d{1,2}\\-\\d{1,2}\\-\\d{4} \\d{2}:\\d{2}:\\d{2}\\]\\s)?[$!](?<MessageType>[A-Z]{5}).*?(?>\\\\0Dh|\\\\0Ah){1,2}", RegexOptions.Singleline);
            var cleanup = new Regex("(\\[\\d{1,2}\\-\\d{1,2}\\-\\d{4} \\d{2}:\\d{2}:\\d{2}\\] |\\\\0Dh|\\\\0Ah|\\n|\\r)");
            var payload = new Regex("[!$]AI(?>VDO|VDM),(?<Fragments>\\d),(?<MessageNumber>\\d),(?<Sequence>\\d?),.?,(?<Payload>.+),\\d?\\*[A-F\\d]{2}");
            
            var matchedLines = dataLines.Matches(lines);
            var parsedLines = new List<string>();
            var lastMessageTime = string.Empty;
            var concatenatedPayload = string.Empty;

            foreach (Match m in matchedLines)
            {
                if (m.Groups["MessageTime"].Value != string.Empty)
                    lastMessageTime = m.Groups["MessageTime"].Value;


                var cleanedUpRow = cleanup.Replace(m.Value, string.Empty);

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

            System.IO.File.WriteAllLines("c:\\temp\\muxlog-fixed.txt", parsedLines);
        }


    }
}
