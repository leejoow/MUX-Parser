using System.Collections.Generic;
using System.Xml.Serialization;

namespace ConvertToGPX.DTO.OUT
{
    [XmlRoot("gpx", Namespace = "http://www.topografix.com/GPX/1/1")]
    public class Gpx
    {
        public Gpx()
        {
            Version = "1.1";
        }

        [XmlAttribute("version")]
        public string Version { get; set; }
        
        [XmlAttribute("creator")]
        public string Creator { get; set; }

        [XmlElement("trk")]
        public List<Track> Tracks { get; set; }
    }
}
