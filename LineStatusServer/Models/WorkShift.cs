using System;

namespace LineStatusServer.Models
{
    public class WorkShift
    {
        public int ID { get; set; }
        public int ShiftCode { get; set; }
        public string ShiftName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

}
