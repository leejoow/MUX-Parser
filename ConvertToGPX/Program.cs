using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace ConvertToGPX
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = System.IO.File.ReadAllLines("c:\\temp\\muxlog-fixed.txt");

            var regex = new Regex("GPGLL,(?<N>\\d{4}\\.\\d{5}),[NS],(?<E>\\d{5}\\.\\d{5}),[EW],(?<Time>\\d{6}).*\\*[A-F\\d]{2}");

            var Gpx = new DTO.OUT.Gpx()
            {
                Tracks = new List<DTO.OUT.Track>()
                {
                   new DTO.OUT.Track()
                   {
                       Name = "GPGLL Track",
                       TrackSegments = new List<DTO.OUT.TrackSegment>()
                       {
                           new DTO.OUT.TrackSegment()
                           {
                               TrackPoints = new List<DTO.OUT.TrackPoint>()
                           }
                       }
                   }
                }
            };
                


            foreach (var line in lines)
            {
                var match = regex.Match(line);

                if (match.Success)
                {
                    var latitude = Convert.ToDecimal(match.Groups["N"].Value.Substring(0, 2)) +
                        Convert.ToDecimal(match.Groups["N"].Value.Substring(2, 8), CultureInfo.InvariantCulture) / 60;
                    var longitude = Convert.ToDecimal(match.Groups["E"].Value.Substring(0, 3)) +
                        Convert.ToDecimal(match.Groups["E"].Value.Substring(3, 8), CultureInfo.InvariantCulture) / 60;

                    Gpx.Tracks[0].TrackSegments[0].TrackPoints.Add(new DTO.OUT.TrackPoint()
                    {
                        Latitude = latitude,
                        Longitude = longitude
                    }) ;
                }
            }

            var xs = new XmlSerializer(Gpx.GetType());
            using (var tw = new System.IO.StreamWriter("c:\\temp\\leo.gpx"))
            {
                xs.Serialize(tw, Gpx);
                tw.Flush();
            }



            Console.ReadLine();
        }
    }
}
