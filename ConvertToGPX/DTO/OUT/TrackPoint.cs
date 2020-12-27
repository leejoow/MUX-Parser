using System;
using System.Xml.Serialization;

namespace ConvertToGPX.DTO.OUT
{
    public class TrackPoint
    {
        [XmlAttribute("lat")]
        public decimal Latitude { get; set; }

        [XmlAttribute("lon")]
        public decimal Longitude { get; set; }
        
        [XmlElement("time")]
        public DateTime Time { get; set; }
    }
}
