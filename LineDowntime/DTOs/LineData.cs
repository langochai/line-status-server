using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineDowntime.DTOs
{

    public class LineData
    {
        public string LineCode { get; set; }
        public int Status { get; set; }
        public DateTime Timestamp { get; set; }
        public int ProductCount { get; set; }
        
    }

}
