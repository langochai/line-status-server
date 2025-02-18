using System;

namespace LineStatusServer.DTOs
{
    public class LineData
    {
        public string LineCode { get; set; }
        public int Status { get; set; }
        public DateTime Timestamp { get; set; }
        public int ProductCount { get; set; }
        public int shift { get; set; }
    }
}