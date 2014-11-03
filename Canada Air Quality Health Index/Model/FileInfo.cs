using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canada_Air_Quality_Health_Index
{
    public class FileInfo
    {
        public string nameEn { get; set; }
        public string cgndb { get; set; }
        public string obsLink { get; set; }
        public string forecLink { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }
        public string Observation { get; set; }
        public string Forecast { get; set; }

    }
}
