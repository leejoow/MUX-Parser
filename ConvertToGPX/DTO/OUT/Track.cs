using System.Collections.Generic;
using System.Xml.Serialization;

namespace ConvertToGPX.DTO.OUT
{
    public class Track
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("trkseg")]
        public List<TrackSegment> TrackSegments { get; set; }
    }
}
