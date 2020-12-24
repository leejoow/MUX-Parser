using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MUX_Parser
{
    public class AISDecode
    {
        public string DecodeAISMessage(string message)
        {
            var match = Regex.Match(message, "!AI(?>VDO|VDM),\\d,\\d,\\d?,.?,(?<Payload>.+),\\d?\\*[A-F\\d]{2}");
            
            if (!match.Success)
                throw new Exception("Not a valid AIS NMEA string");

            var bits = ConvertPayloadToBitArray(GetPayloadFromMessage(match.Groups["Payload"].ToString()));

            var messageType = ConvertBitsToInt(bits, 0, 6);
            switch (messageType)
            {
                case 18:
                    return DecodeMessageType18(bits);
                case 24:
                    return string.Empty;
                default:
                    return string.Empty;
                    //throw new NotImplementedException($"MessageType {messageType} not implemented");
            }
        }

        public string DecodeMessageType18(BitArray bitArray)
        {
            var mmsi = ConvertBitsToInt(bitArray, 8, 30);
            var sog = ConvertBitsToInt(bitArray, 46, 10) / 10.0;
            var cog = ConvertBitsToInt(bitArray, 112, 12) / 10.0;
            var latitude = ConvertBitsToInt(bitArray, 85, 27) / 600000.0;
            var longitude = ConvertBitsToInt(bitArray, 57, 28) / 600000.0;

            return $"MMSI: {mmsi}; SOG: {sog}; COG: {cog}; Lat: {Math.Floor(latitude)}\" {(latitude % 1) * 60}' ; Long: {Math.Floor(longitude)}\" {(longitude % 1) * 60}'";
        }

        public List<int> GetPayloadFromMessage(string message)
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

            return payload;
        }

        public BitArray ConvertPayloadToBitArray(List<int> payload)
        {
            BitArray bits = new BitArray(payload.Count * 6);

            for (var i = 0; i < payload.Count; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    bits[j + i * 6] = ((payload[i] << j) & 32) == 32;
                }
            }

            return bits;
        }

        public int ConvertBitsToInt(BitArray bitArray, int startPos, int length)
        {
            var returnValue = default(int);

            for (int i = startPos; i < startPos + length; i++)
            {
                returnValue <<= 1;
                returnValue |= (bitArray[i] ? 1 : 0);
            }

            return returnValue;
        }
    }
}
