using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MUX_Parser
{
    public class AISDecode
    {
        public string DecodeAISPayload(string payload)
        {

            var bits = ConvertPayloadToBitArray(GetPayloadFromMessage(payload));

            var messageType = ConvertBitsToInt(bits, 0, 6);
            switch (messageType)
            {
                case 1:
                case 2:
                case 3:
                    return DecodePositionReportClassA(bits);
                case 4:
                    return DecodeBaseStationReport(bits);
                case 5:
                    return DecodeStaticAndVoyagedRelatedDataReport(bits);
                case 18:
                    return DecodeStandardClassBPositionReport(bits);
                case 24:
                    return DecodeStaticDataReport(bits);
                default:
                    return $" *** FAIL *** MessageType {messageType} not implemented";
            }
        }

        public string DecodeBaseStationReport(BitArray bitArray)
        {
            var mmsi = ConvertBitsToInt(bitArray, 8, 30);
            var latitude = ConvertBitsToInt(bitArray, 107, 27) / 600000.0;
            var longitude = ConvertBitsToInt(bitArray, 79, 28) / 600000.0;
            var year = ConvertBitsToInt(bitArray, 38, 14);
            var month = ConvertBitsToInt(bitArray, 52, 4);
            var day = ConvertBitsToInt(bitArray, 56, 5);
            var hour = ConvertBitsToInt(bitArray, 61, 5);
            var minute = ConvertBitsToInt(bitArray, 66, 6);
            var second = ConvertBitsToInt(bitArray, 72, 6);

            return $" *** {MethodBase.GetCurrentMethod().Name} *** MMSI: {mmsi}; Lat: {Math.Floor(latitude)}\" {(latitude % 1) * 60}' ; Long: {Math.Floor(longitude)}\" {(longitude % 1) * 60}'; Date: {year}-{month}-{day}; Time: {hour}:{minute}:{second}";
        }

        public string DecodePositionReportClassA(BitArray bitArray)
        {
            var mmsi = ConvertBitsToInt(bitArray, 8, 30);
            var sog = ConvertBitsToInt(bitArray, 50, 10) / 10.0;
            var cog = ConvertBitsToInt(bitArray, 116, 12) / 10.0;
            var latitude = ConvertBitsToInt(bitArray, 89, 27) / 600000.0;
            var longitude = ConvertBitsToInt(bitArray, 61, 28) / 600000.0;

            return $" *** {MethodBase.GetCurrentMethod().Name} *** MMSI: {mmsi}; SOG: {sog}; COG: {cog}; Lat: {Math.Floor(latitude)}\" {(latitude % 1) * 60}' ; Long: {Math.Floor(longitude)}\" {(longitude % 1) * 60}'";
        }

        public string DecodeStandardClassBPositionReport(BitArray bitArray)
        {
            var mmsi = ConvertBitsToInt(bitArray, 8, 30);
            var sog = ConvertBitsToInt(bitArray, 46, 10) / 10.0;
            var cog = ConvertBitsToInt(bitArray, 112, 12) / 10.0;
            var latitude = ConvertBitsToInt(bitArray, 85, 27) / 600000.0;
            var longitude = ConvertBitsToInt(bitArray, 57, 28) / 600000.0;

            return $" *** {MethodBase.GetCurrentMethod().Name} *** MMSI: {mmsi}; SOG: {sog}; COG: {cog}; Lat: {Math.Floor(latitude)}\" {(latitude % 1) * 60}' ; Long: {Math.Floor(longitude)}\" {(longitude % 1) * 60}'";
        }

        public string DecodeStaticDataReport(BitArray bitArray)
        {
            var partSpecificString = string.Empty;
            
            var mmsi = ConvertBitsToInt(bitArray, 8, 30);
            var partNumber = ConvertBitsToInt(bitArray, 38, 2);

            switch (partNumber)
            {
                case 0:
                    var vesselName = ConvertBitsToString(bitArray, 40, 120);

                    partSpecificString = $"VesselName: {vesselName}";

                    break;
                case 1:
                    var dimensionToBow = ConvertBitsToInt(bitArray, 132, 9);
                    var dimensionToStern = ConvertBitsToInt(bitArray, 141, 9);
                    var dimensionToPort = ConvertBitsToInt(bitArray, 150, 6);
                    var dimensionToStarboard = ConvertBitsToInt(bitArray, 156, 6);
                    var shipType = ConvertBitsToShipType(bitArray, 40, 8);
                    var callSign = ConvertBitsToString(bitArray, 90, 42);

                    partSpecificString = $"CallSign: {callSign}; ShipType: {shipType}; DimensionToBow: {dimensionToBow}; DimensionToStern: {dimensionToStern}; DimensionToPort: {dimensionToPort}; DimensionToStarboard: {dimensionToStarboard}";

                    break;
                default:
                    throw new NotImplementedException("Only Part A and B are implemented");
            }

            return $" *** {MethodBase.GetCurrentMethod().Name} *** MMSI: {mmsi}; PartNumber: {partNumber}; {partSpecificString}";
        }

        public string DecodeStaticAndVoyagedRelatedDataReport(BitArray bitArray)
        {
            var partSpecificString = string.Empty;

            var mmsi = ConvertBitsToInt(bitArray, 8, 30);

            var vesselName = ConvertBitsToString(bitArray, 112, 120);
            var imoNumber = ConvertBitsToInt(bitArray, 40, 30);
            var callSign = ConvertBitsToString(bitArray, 70, 42);
            var shipType = ConvertBitsToShipType(bitArray, 232, 8);
            var dimensionToBow = ConvertBitsToInt(bitArray, 240, 9);
            var dimensionToStern = ConvertBitsToInt(bitArray, 249, 9);
            var dimensionToPort = ConvertBitsToInt(bitArray, 258, 6);
            var dimensionToStarboard = ConvertBitsToInt(bitArray, 264, 6);

            return $" *** {MethodBase.GetCurrentMethod().Name} *** MMSI: {mmsi}; VesselName: {vesselName}; IMO Number: {imoNumber}; Callsign: {callSign}; ShipType: {shipType}; DimensionToBow: {dimensionToBow}; DimensionToStern: {dimensionToStern}; DimensionToPort: {dimensionToPort}; DimensionToStarboard: {dimensionToStarboard}";
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

        public string ConvertBitsToString(BitArray bitArray, int startPos, int length)
        {
            if (length % 6 != 0)
                throw new ArgumentException("Length must be a multiple of 6");

            var returnValue = string.Empty;

            for (int i = startPos; i < startPos + length; i+= 6)
            {
                var nibble = ConvertBitsToInt(bitArray, i, 6);

                if (nibble <= 31)
                    returnValue += Convert.ToChar(nibble + 64);
                else
                    returnValue += Convert.ToChar(nibble);
            }

            return returnValue.TrimEnd('@');
        }

        public string ConvertBitsToShipType(BitArray bitArray, int startPos, int length)
        {
            if (length != 8)
                throw new ArgumentException("Length must be 8");

            var shipTypeEnum = ConvertBitsToInt(bitArray, startPos, length);

            return shipTypeEnum switch
            {
                0 => "Not available (default)",
                1 => "Reserved for future use",
                2 => "Reserved for future use",
                3 => "Reserved for future use",
                4 => "Reserved for future use",
                5 => "Reserved for future use",
                6 => "Reserved for future use",
                7 => "Reserved for future use",
                8 => "Reserved for future use",
                9 => "Reserved for future use",
                10 => "Reserved for future use",
                11 => "Reserved for future use",
                12 => "Reserved for future use",
                13 => "Reserved for future use",
                14 => "Reserved for future use",
                15 => "Reserved for future use",
                16 => "Reserved for future use",
                17 => "Reserved for future use",
                18 => "Reserved for future use",
                19 => "Reserved for future use",
                20 => "Wing in ground (WIG), all ships of this type",
                21 => "Wing in ground (WIG), Hazardous category A",
                22 => "Wing in ground (WIG), Hazardous category B",
                23 => "Wing in ground (WIG), Hazardous category C",
                24 => "Wing in ground (WIG), Hazardous category D",
                25 => "Wing in ground (WIG), Reserved for future use",
                26 => "Wing in ground (WIG), Reserved for future use",
                27 => "Wing in ground (WIG), Reserved for future use",
                28 => "Wing in ground (WIG), Reserved for future use",
                29 => "Wing in ground (WIG), Reserved for future use",
                30 => "Fishing",
                31 => "Towing",
                32 => "Towing: length exceeds 200m or breadth exceeds 25m",
                33 => "Dredging or underwater ops",
                34 => "Diving ops",
                35 => "Military ops",
                36 => "Sailing",
                37 => "Pleasure Craft",
                38 => "Reserved",
                39 => "Reserved",
                40 => "High speed craft (HSC), all ships of this type",
                41 => "High speed craft (HSC), Hazardous category A",
                42 => "High speed craft (HSC), Hazardous category B",
                43 => "High speed craft (HSC), Hazardous category C",
                44 => "High speed craft (HSC), Hazardous category D",
                45 => "High speed craft (HSC), Reserved for future use",
                46 => "High speed craft (HSC), Reserved for future use",
                47 => "High speed craft (HSC), Reserved for future use",
                48 => "High speed craft (HSC), Reserved for future use",
                49 => "High speed craft (HSC), No additional information",
                50 => "Pilot Vessel",
                51 => "Search and Rescue vessel",
                52 => "Tug",
                53 => "Port Tender",
                54 => "Anti-pollution equipment",
                55 => "Law Enforcement",
                56 => "Spare - Local Vessel",
                57 => "Spare - Local Vessel",
                58 => "Medical Transport",
                59 => "Noncombatant ship according to RR Resolution No. 18",
                60 => "Passenger, all ships of this type",
                61 => "Passenger, Hazardous category A",
                62 => "Passenger, Hazardous category B",
                63 => "Passenger, Hazardous category C",
                64 => "Passenger, Hazardous category D",
                65 => "Passenger, Reserved for future use",
                66 => "Passenger, Reserved for future use",
                67 => "Passenger, Reserved for future use",
                68 => "Passenger, Reserved for future use",
                69 => "Passenger, No additional information",
                70 => "Cargo, all ships of this type",
                71 => "Cargo, Hazardous category A",
                72 => "Cargo, Hazardous category B",
                73 => "Cargo, Hazardous category C",
                74 => "Cargo, Hazardous category D",
                75 => "Cargo, Reserved for future use",
                76 => "Cargo, Reserved for future use",
                77 => "Cargo, Reserved for future use",
                78 => "Cargo, Reserved for future use",
                79 => "Cargo, No additional information",
                80 => "Tanker, all ships of this type",
                81 => "Tanker, Hazardous category A",
                82 => "Tanker, Hazardous category B",
                83 => "Tanker, Hazardous category C",
                84 => "Tanker, Hazardous category D",
                85 => "Tanker, Reserved for future use",
                86 => "Tanker, Reserved for future use",
                87 => "Tanker, Reserved for future use",
                88 => "Tanker, Reserved for future use",
                89 => "Tanker, No additional information",
                90 => "Other Type, all ships of this type",
                91 => "Other Type, Hazardous category A",
                92 => "Other Type, Hazardous category B",
                93 => "Other Type, Hazardous category C",
                94 => "Other Type, Hazardous category D",
                95 => "Other Type, Reserved for future use",
                96 => "Other Type, Reserved for future use",
                97 => "Other Type, Reserved for future use",
                98 => "Other Type, Reserved for future use",
                99 => "Other Type, no additional information",
                _ => "Not supported",
            };
        }
    }
}
