using System.Collections.Generic;
using System.Xml.Serialization;

namespace ConvertToGPX.DTO.OUT
{
    public class TrackSegment
    {
        [XmlElement("trkpt")]
        public List<TrackPoint> TrackPoints { get; set; }
    }
}
