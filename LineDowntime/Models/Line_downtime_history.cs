using System;

namespace LineDowntime.Models
{
    public class Line_downtime_history
    {
        public string line_code { get; set; }
        public DateTime timestamp { get; set; }
        public int product_count { get; set; }
        public int status { get; set; }
    }
}